using System.Data.SqlClient;
using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt
{
    public class AdminHandling
    {

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
        public static Administrator RegisterAdmin(AdminEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Email))
                throw new ArgumentException("Email je povinný");

            AdministratorDAO dao = new();
            Administrator admin = new(entry.Jmeno, entry.Prijmeni, entry.Email);

            if (!CheckAdminExists(admin.Email))
                dao.Create(admin);

            return admin;
        }

        public static void RemoveAdmin(int adminId)
        {
            RequestProcessingDAO rpDao = new();
            AdministratorDAO adminDao = new();

            rpDao.ClearAdminAssignments(adminId);

            adminDao.Delete(adminId);
        }
        public static void UpdateAdmin(int adminId,AdministratorUpdateDto dto)
        {
            AdministratorDAO adminDao = new();
            RequestProcessingDAO rpDao = new();

            Administrator? admin = adminDao.GetById(adminId);
            if (admin == null)
                throw new InvalidOperationException("Admin neexistuje");

            if (dto.ReleaseActiveRequests)
            {
                rpDao.ClearAdminAssignments(adminId);
            }

            admin.FirstName = dto.FirstName;
            admin.LastName = dto.LastName;
            admin.Email = dto.Email;

            adminDao.Update(admin);
        }


    }
    public class AdministratorUpdateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool ReleaseActiveRequests { get; set; }
    }

}
