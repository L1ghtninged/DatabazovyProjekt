using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class RequestStatusDAO
    {
        public void Create(RequestStatus status)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                insert into requeststatus (status_text)
                values (@status_text);
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@status_text", status.StatusText);
            status.Id = (int)cmd.ExecuteScalar();

            
        }

        public RequestStatus? GetById(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                select id, status_text
                from requeststatus
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;
            return new RequestStatus(
                reader.GetInt32(0),
                reader.GetString(1)
            );
        }

        public void Update(RequestStatus status)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                update requeststatus
                set status_text = @status_text
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", status.Id);
            cmd.Parameters.AddWithValue("@status_text", status.StatusText);
            cmd.ExecuteNonQuery();
        }
        

        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "delete from requeststatus where id = @id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}