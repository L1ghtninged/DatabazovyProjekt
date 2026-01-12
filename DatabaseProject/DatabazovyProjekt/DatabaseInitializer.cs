namespace DatabazovyProjekt
{
    using System;
    using System.Data.SqlClient;
    using System.IO;

    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            Console.WriteLine("Initializing database...");

            using var conn = DatabaseFactory.CreateConnection();
            conn.Open();

            try
            {
                if (!DatabaseHasTables(conn))
                {
                    Console.WriteLine("Database is empty → creating schema...");
                    ExecuteSql(conn, GetSchemaSql());
                }
                else
                {
                    Console.WriteLine("Database schema already exists.");
                }

                Console.WriteLine("Applying seed data...");
                ExecuteSql(conn, GetSeedSql());

                Console.WriteLine("Applying views...");
                ExecuteSql(conn, GetViewsSql());

                Console.WriteLine("Database initialization finished successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Database initialization failed:");
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // ==============================
        // Helpers
        // ==============================

        private static bool DatabaseHasTables(SqlConnection conn)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM sys.tables 
                WHERE name IN ('Administrator', 'Contact', 'ServiceRequest')";

            using var cmd = new SqlCommand(sql, conn);
            return (int)cmd.ExecuteScalar() > 0;
        }

        private static void ExecuteSql(SqlConnection conn, string sql)
        {
            var commands = sql.Split(
                new[] { "\r\nGO\r\n", "\nGO\n", "\rGO\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var command in commands)
            {
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                using var cmd = new SqlCommand(command, conn);
                cmd.CommandTimeout = 60;
                cmd.ExecuteNonQuery();
            }
        }

        // ==============================
        // SQL: Schema
        // ==============================

        private static string GetSchemaSql() => @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Administrator')
BEGIN
    CREATE TABLE Administrator (
        id INT IDENTITY PRIMARY KEY,
        first_name VARCHAR(50) NOT NULL,
        last_name VARCHAR(50) NOT NULL,
        email VARCHAR(100) NOT NULL UNIQUE,
        admin_role VARCHAR(50) NOT NULL DEFAULT 'AIAgent'
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contact')
BEGIN
    CREATE TABLE Contact (
        id INT IDENTITY PRIMARY KEY,
        first_name VARCHAR(50) NOT NULL,
        last_name VARCHAR(50) NOT NULL,
        email VARCHAR(100) NOT NULL UNIQUE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestStatus')
BEGIN
    CREATE TABLE RequestStatus (
        id INT PRIMARY KEY,
        status_text VARCHAR(20) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceRequest')
BEGIN
    CREATE TABLE ServiceRequest (
        id INT IDENTITY PRIMARY KEY,
        contact_id INT NOT NULL,
        status_id INT NOT NULL,
        request_text VARCHAR(MAX) NOT NULL,
        created_date DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ServiceRequest_Contact FOREIGN KEY (contact_id) REFERENCES Contact(id),
        CONSTRAINT FK_ServiceRequest_Status FOREIGN KEY (status_id) REFERENCES RequestStatus(id)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestProcessing')
BEGIN
    CREATE TABLE RequestProcessing (
        id INT IDENTITY PRIMARY KEY,
        admin_id INT NOT NULL,
        request_id INT NOT NULL,
        started_date DATETIME NOT NULL DEFAULT GETDATE(),
        ended_date DATETIME NULL,
        response_text VARCHAR(MAX),
        CONSTRAINT FK_RequestProcessing_Admin FOREIGN KEY (admin_id) REFERENCES Administrator(id),
        CONSTRAINT FK_RequestProcessing_Request FOREIGN KEY (request_id) REFERENCES ServiceRequest(id)
    );
END
";

        // ==============================
        // SQL: Seed
        // ==============================

        private static string GetSeedSql() => @"
-- Pokud existují requesty, NESMAZÁVÁME, jen opravíme data
IF EXISTS (SELECT 1 FROM ServiceRequest)
BEGIN
    PRINT 'ServiceRequest exists → syncing RequestStatus';

    MERGE RequestStatus AS target
    USING (VALUES
        (1, 'Novy'),
        (2, 'ResiSe'),
        (3, 'Uzavreny'),
        (4, 'Storno')
    ) AS source (id, status_text)
    ON target.id = source.id
    WHEN MATCHED AND target.status_text <> source.status_text THEN
        UPDATE SET status_text = source.status_text
    WHEN NOT MATCHED THEN
        INSERT (id, status_text)
        VALUES (source.id, source.status_text);

    -- Odstranění případných starých statusů
    DELETE FROM RequestStatus
    WHERE id NOT IN (1,2,3,4);
END
ELSE
BEGIN
    PRINT 'No ServiceRequest → recreating RequestStatus';

    DELETE FROM RequestStatus;

    INSERT INTO RequestStatus (id, status_text)
    VALUES
        (1, 'Novy'),
        (2, 'ResiSe'),
        (3, 'Uzavreny'),
        (4, 'Storno');
END
";


        // ==============================
        // SQL: Views
        // ==============================

        private static string GetViewsSql() => @"
IF OBJECT_ID('view_request_overview', 'V') IS NOT NULL
    DROP VIEW view_request_overview;
GO
CREATE VIEW view_request_overview AS
SELECT 
    sr.id AS request_id,
    c.first_name AS contact_first_name,
    c.last_name AS contact_last_name,
    c.email AS contact_email,
    sr.request_text,
    rs.status_text AS status,
    sr.created_date,
    a.first_name AS assigned_admin_first_name,
    a.last_name AS assigned_admin_last_name,
    a.email AS assigned_admin_email,
    rp.started_date,
    rp.ended_date,
    rp.response_text
FROM ServiceRequest sr
JOIN Contact c ON sr.contact_id = c.id
JOIN RequestStatus rs ON sr.status_id = rs.id
LEFT JOIN RequestProcessing rp ON rp.request_id = sr.id AND rp.ended_date IS NULL
LEFT JOIN Administrator a ON rp.admin_id = a.id;
GO

IF OBJECT_ID('view_admin_statistics', 'V') IS NOT NULL
    DROP VIEW view_admin_statistics;
GO
CREATE VIEW view_admin_statistics AS
SELECT 
    a.id AS admin_id,
    a.first_name,
    a.last_name,
    a.email,
    a.admin_role,
    COUNT(CASE WHEN rs.status_text = 'Novy' THEN 1 END) AS new_requests,
    COUNT(CASE WHEN rs.status_text = 'ResiSe' THEN 1 END) AS assigned_requests,
    COUNT(CASE WHEN rs.status_text = 'Uzavreny' THEN 1 END) AS completed_requests,
    COUNT(CASE WHEN rs.status_text = 'Storno' THEN 1 END) AS cancelled_requests,
    COUNT(DISTINCT rp.request_id) AS total_processed,
    AVG(DATEDIFF(MINUTE, rp.started_date, rp.ended_date)) AS avg_processing_time_minutes
FROM Administrator a
LEFT JOIN RequestProcessing rp ON a.id = rp.admin_id
LEFT JOIN ServiceRequest sr ON rp.request_id = sr.id
LEFT JOIN RequestStatus rs ON sr.status_id = rs.id
GROUP BY a.id, a.first_name, a.last_name, a.email, a.admin_role;
GO
";
    }
}
