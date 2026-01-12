using System.Data;
using System.Data.SqlClient;
using DatabazovyProjekt.DTO;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.DAO
{
    public class AdminStatisticsDAO
    {
        /// <summary>
        /// Retrieves statistics for all administrators.
        /// </summary>
        public List<AdminStatisticsDTO> GetAllStatistics()
        {
            using var connection = DatabaseFactory.CreateConnection();
            connection.Open();

            var sql = @"
                select 
                    a.id as admin_id,
                    a.first_name,
                    a.last_name,
                    a.email,
                    a.admin_role,
                    count(case when sr.status_id = 1 then 1 end) as new_requests,
                    count(case when sr.status_id = 2 then 1 end) as assigned_requests,
                    count(case when sr.status_id = 3 then 1 end) as completed_requests,
                    count(case when sr.status_id = 4 then 1 end) as cancelled_requests,
                    count(distinct rp.request_id) as total_processed,
                    isnull(avg(
                        case 
                            when rp.ended_date is not null and rp.started_date is not null
                            then datediff(minute, rp.started_date, rp.ended_date)
                        end
                    ), 0) as avg_processing_time_minutes
                from administrator a
                left join requestprocessing rp on a.id = rp.admin_id
                left join servicerequest sr on rp.request_id = sr.id
                group by a.id, a.first_name, a.last_name, a.email, a.admin_role
                order by total_processed desc";

            using var cmd = new SqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            List<AdminStatisticsDTO> stats = new();

            while (reader.Read())
            {
                try
                {
                    var stat = new AdminStatisticsDTO
                    {
                        AdminId = SafeGetInt(reader, "admin_id"),
                        FirstName = SafeGetString(reader, "first_name"),
                        LastName = SafeGetString(reader, "last_name"),
                        Email = SafeGetString(reader, "email"),
                        AdminRole = SafeGetString(reader, "admin_role", "AIAgent"),
                        NewRequests = SafeGetInt(reader, "new_requests"),
                        AssignedRequests = SafeGetInt(reader, "assigned_requests"),
                        CompletedRequests = SafeGetInt(reader, "completed_requests"),
                        CancelledRequests = SafeGetInt(reader, "cancelled_requests"),
                        TotalProcessed = SafeGetInt(reader, "total_processed"),
                        AvgProcessingTimeMinutes = SafeGetInt(reader, "avg_processing_time_minutes")
                    };

                    stats.Add(stat);
                }
                catch
                {
                }
            }

            return stats;
        }

        /// <summary>
        /// Retrieves statistics for a specific administrator by ID.
        /// </summary>
        public AdminStatisticsDTO? GetStatisticsByAdminId(int adminId)
        {
            using var connection = DatabaseFactory.CreateConnection();
            connection.Open();

            var sql = @"
                select 
                    a.id as admin_id,
                    a.first_name,
                    a.last_name,
                    a.email,
                    a.admin_role,
                    count(case when sr.status_id = 1 then 1 end) as new_requests,
                    count(case when sr.status_id = 2 then 1 end) as assigned_requests,
                    count(case when sr.status_id = 3 then 1 end) as completed_requests,
                    count(case when sr.status_id = 4 then 1 end) as cancelled_requests,
                    count(distinct rp.request_id) as total_processed,
                    isnull(avg(
                        case 
                            when rp.ended_date is not null and rp.started_date is not null
                            then datediff(minute, rp.started_date, rp.ended_date)
                        end
                    ), 0) as avg_processing_time_minutes
                from administrator a
                left join requestprocessing rp on a.id = rp.admin_id
                left join servicerequest sr on rp.request_id = sr.id
                where a.id = @AdminId
                group by a.id, a.first_name, a.last_name, a.email, a.admin_role";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            try
            {
                return new AdminStatisticsDTO
                {
                    AdminId = SafeGetInt(reader, "admin_id"),
                    FirstName = SafeGetString(reader, "first_name"),
                    LastName = SafeGetString(reader, "last_name"),
                    Email = SafeGetString(reader, "email"),
                    AdminRole = SafeGetString(reader, "admin_role", "AIAgent"),
                    NewRequests = SafeGetInt(reader, "new_requests"),
                    AssignedRequests = SafeGetInt(reader, "assigned_requests"),
                    CompletedRequests = SafeGetInt(reader, "completed_requests"),
                    CancelledRequests = SafeGetInt(reader, "cancelled_requests"),
                    TotalProcessed = SafeGetInt(reader, "total_processed"),
                    AvgProcessingTimeMinutes = SafeGetInt(reader, "avg_processing_time_minutes")
                };
            }
            catch
            {
                return null;
            }
        }

        private int SafeGetInt(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return 0;

            var fieldType = reader.GetFieldType(ordinal);

            if (fieldType == typeof(int))
                return reader.GetInt32(ordinal);
            else if (fieldType == typeof(long))
                return (int)reader.GetInt64(ordinal);
            else if (fieldType == typeof(decimal))
                return (int)reader.GetDecimal(ordinal);
            else if (fieldType == typeof(double))
                return (int)reader.GetDouble(ordinal);
            else if (fieldType == typeof(float))
                return (int)reader.GetFloat(ordinal);
            else
                return Convert.ToInt32(reader[ordinal]);
        }

        private string SafeGetString(SqlDataReader reader, string columnName, string defaultValue = "")
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return defaultValue;

            return reader.GetString(ordinal);
        }
    }
}
