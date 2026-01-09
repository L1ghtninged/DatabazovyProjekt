using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class ServiceRequestDAO
    {
        public void Create(ServiceRequest request)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                insert into servicerequest (contact_id, status_id, request_text)
                output inserted.id
                values (@contact_id, @status_id, @request_text);
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@contact_id", request.ContactId);
            cmd.Parameters.AddWithValue("@status_id", request.StatusId);
            cmd.Parameters.AddWithValue("@request_text", request.RequestText);
            request.Id = (int)cmd.ExecuteScalar();
        }

        public ServiceRequest? GetById(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                select id, contact_id, status_id, request_text, created_date
                from servicerequest
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;
            return new ServiceRequest(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetDateTime(4)
            );
        }
        public void UpdateStatus(int serviceRequestId, State newState)
        {
            if (serviceRequestId <= 0)
                throw new ArgumentException("Neplatné ID requestu");

            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
            update servicerequest
            set status_id = @status_id
            where id = @id;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", serviceRequestId);
            cmd.Parameters.AddWithValue("@status_id", (int)newState);

            cmd.ExecuteNonQuery();
        }
        public void Update(ServiceRequest request)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                update servicerequest
                set contact_id = @contact_id,
                    status_id = @status_id,
                    request_text = @request_text
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", request.Id);
            cmd.Parameters.AddWithValue("@contact_id", request.ContactId);
            cmd.Parameters.AddWithValue("@status_id", request.StatusId);
            cmd.Parameters.AddWithValue("@request_text", request.RequestText);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "delete from servicerequest where id = @id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}