using System.Data.SqlClient;

namespace DatabazovyProjekt.Controllers
{
    public static class ImportController
    {
        /// <summary>
        /// POST /api/import/csv - Imports CSV file into Contact or Administrator table.
        /// </summary>
        public static async Task<IResult> ImportCsv(IFormFile file, string table, bool hasHeader = true)
        {
            try
            {
                if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded");
                if (string.IsNullOrEmpty(table)) return Results.BadRequest("Table name is required");

                var validTables = new[] { "Contact", "Administrator" };
                if (!validTables.Contains(table)) return Results.BadRequest($"Invalid table. Allowed: {string.Join(", ", validTables)}");

                var rowsImported = 0;

                using var stream = new StreamReader(file.OpenReadStream());
                using var conn = DatabaseFactory.CreateConnection();
                conn.Open();

                string line;
                bool isFirstLine = true;

                while ((line = await stream.ReadLineAsync()) != null)
                {
                    if (hasHeader && isFirstLine) { isFirstLine = false; continue; }
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');

                    switch (table.ToLower())
                    {
                        case "contact": await ImportContact(conn, columns); break;
                        case "administrator": await ImportAdministrator(conn, columns); break;
                    }

                    rowsImported++;
                    isFirstLine = false;
                }

                return Results.Ok(new { success = true, table, rows = rowsImported, message = $"Imported {rowsImported} rows into {table}" });
            }
            catch (Exception ex) { return Results.Problem($"Import failed: {ex.Message}"); }
        }

        private static async Task ImportContact(SqlConnection conn, string[] columns)
        {
            if (columns.Length < 3) return;

            var sql = @"
                if not exists (select 1 from contact where email = @email)
                begin
                    insert into contact (first_name, last_name, email) 
                    values (@firstName, @lastName, @email)
                end";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstName", columns[0].Trim());
            cmd.Parameters.AddWithValue("@lastName", columns[1].Trim());
            cmd.Parameters.AddWithValue("@email", columns[2].Trim());

            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task ImportAdministrator(SqlConnection conn, string[] columns)
        {
            if (columns.Length < 3) return;

            var sql = @"
                if not exists (select 1 from administrator where email = @email)
                begin
                    insert into administrator (first_name, last_name, email, admin_role) 
                    values (@firstName, @lastName, @email, 'AIAgent')
                end";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstName", columns[0].Trim());
            cmd.Parameters.AddWithValue("@lastName", columns[1].Trim());
            cmd.Parameters.AddWithValue("@email", columns[2].Trim());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
