using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class AdministratorDAO
    {
        /// <summary>
        /// Inserts a new administrator into the database and sets its ID.
        /// </summary>
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

        /// <summary>
        /// Retrieves an administrator by its ID, or returns null if not found.
        /// </summary>
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

        /// <summary>
        /// Retrieves an administrator by email, or returns null if not found.
        /// </summary>
        public Administrator? GetByEmail(string email)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = @"
                select id, first_name, last_name, email
                from administrator
                where email = @email;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

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

        /// <summary>
        /// Updates an existing administrator's data in the database.
        /// </summary>
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

        /// <summary>
        /// Deletes an administrator by its ID.
        /// </summary>
        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            string sql = "delete from administrator where id = @id;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns a list of active request IDs assigned to a specific administrator.
        /// </summary>
        public List<int> GetActiveRequestIds(int adminId)
        {
            if (adminId <= 0)
                throw new ArgumentException("Invalid administrator ID");

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
