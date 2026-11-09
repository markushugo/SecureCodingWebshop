using Microsoft.Data.SqlClient;

namespace Webshop.Library.Data;

/// <summary>Opretter forbindelser til webshoppens SQL-database.</summary>
public sealed class SqlConnectionFactory(string connectionString)
{
    /// <summary>Opretter en ny, endnu ikke åbnet databaseforbindelse.</summary>
    public SqlConnection Create() => new(connectionString);
}
