using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class RequestProcessingDAO
    {
        public void Create(RequestProcessing processing)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                insert into requestprocessing (admin_id, request_id, response_text)
                values (@admin_id, @request_id, @response_text);
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@admin_id", processing.AdminId);
            cmd.Parameters.AddWithValue("@request_id", processing.RequestId);
            cmd.Parameters.AddWithValue("@response_text", (object?)processing.ResponseText ?? DBNull.Value);
            processing.Id = (int)cmd.ExecuteScalar();

            
        }

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

        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "delete from requestprocessing where id = @id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

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
                throw new InvalidOperationException("Zpracování neexistuje nebo je již ukončeno");
        }
        public void ClearAdminAssignments(int adminId)
        {
            if (adminId <= 0)
                throw new ArgumentException("Neplatné ID admina");

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

    }
}