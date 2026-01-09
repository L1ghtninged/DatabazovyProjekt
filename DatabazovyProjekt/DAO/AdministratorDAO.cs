using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class AdministratorDAO
    {
        public void Create(Administrator administrator)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                insert into administrator (first_name, last_name, email)
                output inserted.id
                values (@first_name, @last_name, @email);
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@first_name", administrator.FirstName);
            cmd.Parameters.AddWithValue("@last_name", administrator.LastName);
            cmd.Parameters.AddWithValue("@email", administrator.Email);

            administrator.Id = (int)cmd.ExecuteScalar();
        }


        public Administrator? GetById(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                select id, first_name, last_name, email
                from administrator
                where id = @id;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Administrator(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            );
        }

        public void Update(Administrator administrator)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                update administrator
                set first_name = @first_name,
                    last_name = @last_name,
                    email = @email
                where id = @id;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", administrator.Id);
            cmd.Parameters.AddWithValue("@first_name", administrator.FirstName);
            cmd.Parameters.AddWithValue("@last_name", administrator.LastName);
            cmd.Parameters.AddWithValue("@email", administrator.Email);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = "delete from administrator where id = @id;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
        public List<int> GetActiveRequestIds(int adminId)
        {
            if (adminId <= 0)
                throw new ArgumentException("Neplatné ID admina");

            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"select request_id from requestprocessing where admin_id = @adminId and ended_date is null;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@adminId", adminId);

            using var reader = cmd.ExecuteReader();

            List<int> requestIds = new();

            while (reader.Read())
            {
                requestIds.Add(reader.GetInt32(0));
            }

            return requestIds;
        }


    }
}
