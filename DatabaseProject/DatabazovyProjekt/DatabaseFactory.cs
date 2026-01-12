using System.Data.SqlClient;

namespace DatabazovyProjekt;

/// <summary>
/// Factory pattern for creating and managing connections
/// </summary>
public static class DatabaseFactory
{
    public static string? connectionString;

    /// <summary>
    /// Initialization based on the appsettings.json file
    /// </summary>
    public static void Init(IConfiguration configuration)
    {
        var db = configuration.GetSection("Database");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = db["DataSource"],
            InitialCatalog = db["Name"],
            TrustServerCertificate = true,
            ConnectTimeout = 30
        };

        bool useWindowsAuth = bool.Parse(db["IntegratedSecurity"] ?? "true");

        if (useWindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = db["User"];
            builder.Password = db["Password"];
            builder.IntegratedSecurity = false;
        }

        connectionString = builder.ConnectionString;
    }

    public static SqlConnection CreateConnection()
    {
        if (connectionString == null)
            throw new InvalidOperationException("DatabaseFactory not initialized");

        return new SqlConnection(connectionString);
    }
}
