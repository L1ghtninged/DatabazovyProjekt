using System.Data.SqlClient;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class ContactDAO
    {
        /// <summary>
        /// Inserts a new contact into the database and sets its ID.
        /// </summary>
        public void Create(Contact contact)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                insert into contact (first_name, last_name, email)
                output inserted.id
                values (@first_name, @last_name, @email);
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@first_name", contact.FirstName);
            cmd.Parameters.AddWithValue("@last_name", contact.LastName);
            cmd.Parameters.AddWithValue("@email", contact.Email);
            contact.Id = (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Retrieves a contact by its ID, or null if not found.
        /// </summary>
        public Contact? GetById(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                select id, first_name, last_name, email
                from contact
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;
            return new Contact(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            );
        }

        /// <summary>
        /// Retrieves a contact by email, or null if not found.
        /// </summary>
        public Contact? GetByEmail(string email)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                select id, first_name, last_name, email
                from contact
                where email = @email;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;
            return new Contact(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            );
        }

        /// <summary>
        /// Updates an existing contact in the database.
        /// </summary>
        public void Update(Contact contact)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = @"
                update contact
                set first_name = @first_name,
                    last_name = @last_name,
                    email = @email
                where id = @id;
            ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", contact.Id);
            cmd.Parameters.AddWithValue("@first_name", contact.FirstName);
            cmd.Parameters.AddWithValue("@last_name", contact.LastName);
            cmd.Parameters.AddWithValue("@email", contact.Email);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a contact by its ID.
        /// </summary>
        public void Delete(int id)
        {
            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();
            string sql = "delete from contact where id = @id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
