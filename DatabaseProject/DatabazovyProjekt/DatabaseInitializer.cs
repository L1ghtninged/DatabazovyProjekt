namespace DatabazovyProjekt
{
    using System.Data.SqlClient;
    using System.IO;

    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                Console.WriteLine("Initializing database...");

                using var conn = DatabaseFactory.CreateConnection();
                conn.Open();

                var sqlPath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseSchema.sql");

                if (!File.Exists(sqlPath))
                {
                    Console.WriteLine($"SQL file not found: {sqlPath}");
                    Console.WriteLine("Creating database manually...");
                    CreateDatabaseManually(conn);
                    return;
                }

                var sql = File.ReadAllText(sqlPath);

                ExecuteSqlScript(conn, sql);

                Console.WriteLine("Database initialized successfully from SQL file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }

        private static void ExecuteSqlScript(SqlConnection conn, string sql)
        {
            var commands = sql.Split(new[] { "GO\r\n", "GO\n", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var command in commands)
            {
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                try
                {
                    using var cmd = new SqlCommand(command, conn);
                    cmd.CommandTimeout = 30;
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException sqlEx)
                {
                    if (!sqlEx.Message.Contains("already exists") &&
                        !sqlEx.Message.Contains("There is already an object named"))
                    {
                        Console.WriteLine($"SQL warning: {sqlEx.Message}");
                    }
                }
            }
        }

        private static void CreateDatabaseManually(SqlConnection conn)
        {
            var manualSql = @"
        -- Tabulky s kontrolou existence
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Administrator')
        BEGIN
            create table Administrator (...);
            PRINT 'Table Administrator created.';
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contact')
        BEGIN
            create table Contact (...);
            PRINT 'Table Contact created.';
        END

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestStatus')
        BEGIN
            create table RequestStatus (...);
            PRINT 'Table RequestStatus created.';
        END

        -- ... všechny ostatní tabulky podobně ...

        -- Insert dat pouze pokud neexistují
        IF NOT EXISTS (SELECT 1 FROM RequestStatus WHERE id = 1)
        BEGIN
            INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (1, N'Novy')
        END

        IF NOT EXISTS (SELECT 1 FROM RequestStatus WHERE id = 2)
        BEGIN
            INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (2, N'ResiSe')
        END

        -- ... nebo elegantněji ...
        MERGE INTO RequestStatus AS target
        USING (VALUES 
            (1, 'Novy'),
            (2, 'ResiSe'),
            (3, 'Uzavreny'),
            (4, 'Storno')
        ) AS source (id, status_text)
        ON target.id = source.id
        WHEN NOT MATCHED THEN
            INSERT (id, status_text) VALUES (source.id, source.status_text);

        -- ... vytvoření pohledů s kontrolou ...
        IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'view_request_overview')
        BEGIN
            EXEC('CREATE VIEW view_request_overview AS SELECT 1 as dummy');
            EXEC('ALTER VIEW view_request_overview AS ... plný kód ...');
        END
    ";

            ExecuteSqlScript(conn, manualSql);
        }
    }
}
