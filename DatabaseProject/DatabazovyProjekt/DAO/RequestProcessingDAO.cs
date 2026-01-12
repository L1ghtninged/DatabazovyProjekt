using System.Data.SqlClient;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt.DTO;

namespace DatabazovyProjekt.DAO
{
    public class RequestProcessingDAO
    {
        /// <summary>
        /// Inserts a new request processing entry and sets its ID.
        /// </summary>
        public void Create(RequestProcessing processing)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                insert into requestprocessing (admin_id, request_id, response_text)
                output inserted.id
                values (@admin_id, @request_id, @response_text);
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@admin_id", processing.AdminId);
            cmd.Parameters.AddWithValue("@request_id", processing.RequestId);
            cmd.Parameters.AddWithValue("@response_text", (object?)processing.ResponseText ?? DBNull.Value);
            processing.Id = (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Retrieves a request processing entry by its ID, or null if not found.
        /// </summary>
        public RequestProcessing? GetById(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                select id, admin_id, request_id, started_date, ended_date, response_text
                from requestprocessing
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new RequestProcessing(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetDateTime(3),
                reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                reader.IsDBNull(5) ? null : reader.GetString(5)
            );
        }

        /// <summary>
        /// Updates an existing request processing entry.
        /// </summary>
        public void Update(RequestProcessing processing)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                update requestprocessing
                set admin_id = @admin_id,
                    request_id = @request_id,
                    ended_date = @ended_date,
                    response_text = @response_text
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", processing.Id);
            cmd.Parameters.AddWithValue("@admin_id", processing.AdminId);
            cmd.Parameters.AddWithValue("@request_id", processing.RequestId);
            cmd.Parameters.AddWithValue("@ended_date", (object?)processing.EndedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@response_text", (object?)processing.ResponseText ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a request processing entry by its ID.
        /// </summary>
        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "delete from requestprocessing where id = @id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Marks a request processing entry as finished with a response text.
        /// </summary>
        public void Finish(int requestProcessingId, string responseText)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                update requestprocessing
                set ended_date = getdate(),
                    response_text = @text
                where id = @id and ended_date is null;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", requestProcessingId);
            cmd.Parameters.AddWithValue("@text", responseText);

            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Processing does not exist or is already finished");
        }

        /// <summary>
        /// Clears all active assignments for a specific administrator.
        /// </summary>
        public void ClearAdminAssignments(int adminId)
        {
            if (adminId <= 0)
                throw new ArgumentException("Invalid administrator ID");

            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                update requestprocessing
                set admin_id = null
                where admin_id = @adminId
                  and ended_date is null;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@adminId", adminId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Retrieves all active requests assigned to a specific administrator.
        /// </summary>
        public List<ServiceRequest> GetActiveRequestsByAdmin(int adminId)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                select sr.id, sr.contact_id, sr.status_id, sr.request_text, sr.created_date
                from servicerequest sr
                join requestprocessing rp on rp.request_id = sr.id
                where rp.admin_id = @adminId
                  and rp.ended_date is null;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@adminId", adminId);

            using var reader = cmd.ExecuteReader();
            List<ServiceRequest> list = new();

            while (reader.Read())
            {
                list.Add(new ServiceRequest(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2),
                    reader.GetString(3),
                    reader.GetDateTime(4)
                ));
            }

            return list;
        }
    }
}
