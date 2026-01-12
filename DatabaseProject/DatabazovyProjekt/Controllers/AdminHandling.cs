using System.Data.SqlClient;
using DatabazovyProjekt.DAO;
using DatabazovyProjekt.DTO;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.Controllers
{
    public class AdminHandling
    {
        /// <summary>
        /// Checks if an admin exists by email.
        /// </summary>
        public static bool CheckAdminExists(string email)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "select count(*) from administrator where email = @email";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        /// <summary>
        /// Registers a new administrator.
        /// </summary>
        public static Administrator RegisterAdmin(AdminCreateDTO entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Email))
                throw new ArgumentException("Email je povinný");

            AdministratorDAO dao = new();
            Administrator admin = new(entry.FirstName, entry.LastName, entry.Email);

            if (!CheckAdminExists(admin.Email))
                dao.Create(admin);

            return admin;
        }

        /// <summary>
        /// Removes an administrator and releases their assigned requests.
        /// </summary>
        public static void RemoveAdmin(int adminId)
        {
            RequestProcessingDAO rpDao = new();
            AdministratorDAO adminDao = new();
            ServiceRequestDAO srDao = new();

            try
            {
                var assignedRequests = rpDao.GetActiveRequestsByAdmin(adminId);
                debugLog($"Našlo se {assignedRequests?.Count ?? 0} přiřazených requestů pro admina {adminId}");

                if (assignedRequests != null)
                {
                    foreach (var request in assignedRequests)
                    {
                        try
                        {
                            srDao.UpdateStatus(request.Id, State.Novy);
                            debugLog($"Request {request.Id} změněn na stav Nový");

                            using var conn = DatabaseFactory.CreateConnection();
                            conn.Open();
                            string sql = @"
                                UPDATE RequestProcessing 
                                SET ended_date = GETDATE() 
                                WHERE request_id = @requestId 
                                  AND admin_id = @adminId 
                                  AND ended_date IS NULL";

                            using var cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("@requestId", request.Id);
                            cmd.Parameters.AddWithValue("@adminId", adminId);
                            int affected = cmd.ExecuteNonQuery();
                            debugLog($"RequestProcessing uvolněno: {affected} řádků");
                        }
                        catch (Exception ex)
                        {
                            debugLog($"Chyba při zpracování requestu {request.Id}: {ex.Message}");
                        }
                    }
                }

                rpDao.ClearAdminAssignments(adminId);
                adminDao.Delete(adminId);

                debugLog($"Admin {adminId} úspěšně smazán");
            }
            catch (Exception ex)
            {
                debugLog($"Chyba při mazání admina {adminId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates administrator information and optionally releases active requests.
        /// </summary>
        public static void UpdateAdmin(int adminId, AdminUpdateDTO dto)
        {
            AdministratorDAO adminDao = new();
            RequestProcessingDAO rpDao = new();
            ServiceRequestDAO srDao = new();

            Administrator? admin = adminDao.GetById(adminId);
            if (admin == null)
                throw new InvalidOperationException("Admin neexistuje");

            if (dto.ReleaseActiveRequests)
            {
                var assignedRequests = rpDao.GetActiveRequestsByAdmin(adminId);
                foreach (var request in assignedRequests)
                {
                    srDao.UpdateStatus(request.Id, State.Novy);
                }
                rpDao.ClearAdminAssignments(adminId);
            }

            admin.FirstName = dto.FirstName;
            admin.LastName = dto.LastName;
            admin.Email = dto.Email;

            adminDao.Update(admin);
        }

        /// <summary>
        /// Logs in an administrator by email.
        /// </summary>
        public static Administrator LoginAdmin(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email je povinný");

            AdministratorDAO dao = new();
            Administrator? admin = dao.GetByEmail(email);

            if (admin == null)
                throw new UnauthorizedAccessException("Admin neexistuje");

            return admin;
        }

        private static void debugLog(string message)
        {
            Console.WriteLine($"[AdminHandling] {DateTime.Now:HH:mm:ss} - {message}");
        }
    }

    public class AdminResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
