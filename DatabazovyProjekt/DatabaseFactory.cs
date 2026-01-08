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
            UserID = db["User"],
            Password = db["Password"],
            InitialCatalog = db["Name"],
            DataSource = db["DataSource"],
            TrustServerCertificate = true,
            ConnectTimeout = 30,
            IntegratedSecurity = false
        };

        connectionString = builder.ConnectionString;
        
    }

    public static SqlConnection CreateConnection()
    {
        if (connectionString == null)
            throw new InvalidOperationException("DatabaseFactory not initialized");

        return new SqlConnection(connectionString);
    }
}
