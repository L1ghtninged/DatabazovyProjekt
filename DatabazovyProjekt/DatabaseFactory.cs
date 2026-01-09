using System.Data.SqlClient;

namespace DatabazovyProjekt;

/// <summary>
/// Tovarna na databazova pripojeni (Factory pattern)
/// </summary>
public static class DatabaseFactory
{
    public static string? connectionString;

    /// <summary>
    /// Inicializace továrny – zavolat při startu aplikace
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

        // Rozhodnutí podle configu
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
