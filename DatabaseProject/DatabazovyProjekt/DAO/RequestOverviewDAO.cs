using System.Data;
using System.Data.SqlClient;
using DatabazovyProjekt.DTO;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class RequestOverviewDAO
    {
        /// <summary>
        /// Retrieves all requests from the overview view.
        /// </summary>
        public List<RequestOverviewDTO> GetAllRequests()
        {
            using var connection = DatabaseFactory.CreateConnection();
            connection.Open();

            var sql = @"
                select * from view_request_overview 
                order by created_date desc";

            using var cmd = new SqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            List<RequestOverviewDTO> requests = new();

            while (reader.Read())
            {
                requests.Add(new RequestOverviewDTO
                {
                    RequestId = reader.GetInt32(reader.GetOrdinal("request_id")),
                    ContactFirstName = reader.GetString(reader.GetOrdinal("contact_first_name")),
                    ContactLastName = reader.GetString(reader.GetOrdinal("contact_last_name")),
                    ContactEmail = reader.GetString(reader.GetOrdinal("contact_email")),
                    RequestText = reader.GetString(reader.GetOrdinal("request_text")),
                    Status = reader.GetString(reader.GetOrdinal("status")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
                    AssignedAdminFirstName = reader.IsDBNull(reader.GetOrdinal("assigned_admin_first_name")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_first_name")),
                    AssignedAdminLastName = reader.IsDBNull(reader.GetOrdinal("assigned_admin_last_name")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_last_name")),
                    AssignedAdminEmail = reader.IsDBNull(reader.GetOrdinal("assigned_admin_email")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_email")),
                    StartedDate = reader.IsDBNull(reader.GetOrdinal("started_date")) ?
                        null : reader.GetDateTime(reader.GetOrdinal("started_date")),
                    EndedDate = reader.IsDBNull(reader.GetOrdinal("ended_date")) ?
                        null : reader.GetDateTime(reader.GetOrdinal("ended_date")),
                    ResponseText = reader.IsDBNull(reader.GetOrdinal("response_text")) ?
                        null : reader.GetString(reader.GetOrdinal("response_text"))
                });
            }

            return requests;
        }

        /// <summary>
        /// Retrieves requests filtered by status from the overview view.
        /// </summary>
        public List<RequestOverviewDTO> GetRequestsByStatus(string status)
        {
            using var connection = DatabaseFactory.CreateConnection();
            connection.Open();

            var sql = @"
                select * from view_request_overview 
                where status = @Status 
                order by created_date desc";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Status", status);

            using var reader = cmd.ExecuteReader();

            List<RequestOverviewDTO> requests = new();

            while (reader.Read())
            {
                requests.Add(new RequestOverviewDTO
                {
                    RequestId = reader.GetInt32(reader.GetOrdinal("request_id")),
                    ContactFirstName = reader.GetString(reader.GetOrdinal("contact_first_name")),
                    ContactLastName = reader.GetString(reader.GetOrdinal("contact_last_name")),
                    ContactEmail = reader.GetString(reader.GetOrdinal("contact_email")),
                    RequestText = reader.GetString(reader.GetOrdinal("request_text")),
                    Status = reader.GetString(reader.GetOrdinal("status")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
                    AssignedAdminFirstName = reader.IsDBNull(reader.GetOrdinal("assigned_admin_first_name")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_first_name")),
                    AssignedAdminLastName = reader.IsDBNull(reader.GetOrdinal("assigned_admin_last_name")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_last_name")),
                    AssignedAdminEmail = reader.IsDBNull(reader.GetOrdinal("assigned_admin_email")) ?
                        null : reader.GetString(reader.GetOrdinal("assigned_admin_email")),
                    StartedDate = reader.IsDBNull(reader.GetOrdinal("started_date")) ?
                        null : reader.GetDateTime(reader.GetOrdinal("started_date")),
                    EndedDate = reader.IsDBNull(reader.GetOrdinal("ended_date")) ?
                        null : reader.GetDateTime(reader.GetOrdinal("ended_date")),
                    ResponseText = reader.IsDBNull(reader.GetOrdinal("response_text")) ?
                        null : reader.GetString(reader.GetOrdinal("response_text"))
                });
            }

            return requests;
        }
    }
}
