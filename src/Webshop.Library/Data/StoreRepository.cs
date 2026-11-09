using System.Data;
using Microsoft.Data.SqlClient;
using Webshop.Library.Models;

namespace Webshop.Library.Data;

/// <summary>Håndterer webshoppens læsning og skrivning i databasen.</summary>
public sealed class StoreRepository(SqlConnectionFactory factory)
{
    /// <summary>Henter aktive produkter og filtrerer dem efter en valgfri søgetekst.</summary>
    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? search, bool includeInactive = false)
    {
        search = search?.Trim();
        if (search?.Length > 60) throw new ArgumentException("Søgningen må højst være 60 tegn.");
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = """
            SELECT Id, Name, Description, Price, Stock, IsActive
            FROM Product
            WHERE (@includeInactive = 1 OR IsActive = 1)
              AND (@search = N'' OR Name LIKE N'%' + @search + N'%')
            ORDER BY Name;
            """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@includeInactive", SqlDbType.Bit).Value = includeInactive;
        command.Parameters.Add("@search", SqlDbType.NVarChar, 60).Value = search ?? "";
        var products = new List<Product>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) products.Add(ReadProduct(reader));
        return products;
    }

    /// <summary>Henter ét produkt og kan genbruge en eksisterende transaktion.</summary>
    public async Task<Product?> GetProductAsync(int id, bool includeInactive = false, SqlConnection? existingConnection = null, SqlTransaction? transaction = null)
    {
        var ownsConnection = existingConnection is null;
        await using var owned = ownsConnection ? factory.Create() : null;
        var connection = existingConnection ?? owned!;
        if (ownsConnection) await connection.OpenAsync();
        const string sql = """
            SELECT Id, Name, Description, Price, Stock, IsActive
            FROM Product WHERE Id = @id AND (@includeInactive = 1 OR IsActive = 1);
            """;
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@includeInactive", SqlDbType.Bit).Value = includeInactive;
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadProduct(reader) : null;
    }

    /// <summary>Finder en bruger ud fra den normaliserede e-mailadresse.</summary>
    public async Task<AppUser?> FindUserByEmailAsync(string email)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = "SELECT Id, Email, PasswordHash, Role, FailedLoginCount, LockedUntilUtc FROM AppUser WHERE Email = @email;";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadUser(reader) : null;
    }

    /// <summary>Opretter en kunde og returnerer det nye bruger-id.</summary>
    public async Task<int> CreateUserAsync(string email, string passwordHash)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = "INSERT INTO AppUser (Email, PasswordHash, Role) OUTPUT INSERTED.Id VALUES (@email, @hash, 'Customer');";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
        command.Parameters.Add("@hash", SqlDbType.NVarChar, 500).Value = passwordHash;
        return (int)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("Brugeren blev ikke oprettet."));
    }

    /// <summary>Registrerer et mislykket login og spærrer brugeren efter fem forsøg.</summary>
    public async Task RegisterFailedLoginAsync(int userId)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = """
            UPDATE AppUser SET
              FailedLoginCount = FailedLoginCount + 1,
              LockedUntilUtc = CASE WHEN FailedLoginCount + 1 >= 5 THEN DATEADD(MINUTE, 15, SYSUTCDATETIME()) ELSE LockedUntilUtc END
            WHERE Id = @id;
            """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Nulstiller fejlede loginforsøg efter et korrekt login.</summary>
    public async Task ResetFailedLoginAsync(int userId)
    {
        await ExecuteUserUpdateAsync("UPDATE AppUser SET FailedLoginCount = 0, LockedUntilUtc = NULL WHERE Id = @id;", userId);
    }

    /// <summary>Gemmer et nyt hash af brugerens adgangskode.</summary>
    public async Task UpdatePasswordAsync(int userId, string hash)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        await using var command = new SqlCommand("UPDATE AppUser SET PasswordHash = @hash WHERE Id = @id;", connection);
        command.Parameters.Add("@hash", SqlDbType.NVarChar, 500).Value = hash;
        command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Gemmer en sikkerheds- eller brugerhændelse i auditloggen.</summary>
    public async Task WriteAuditAsync(int? userId, string eventType, bool succeeded, string? details = null)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = "INSERT INTO AuditEvent (UserId, EventType, Succeeded, Details) VALUES (@userId, @type, @ok, @details);";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId is null ? DBNull.Value : userId.Value;
        command.Parameters.Add("@type", SqlDbType.NVarChar, 60).Value = eventType;
        command.Parameters.Add("@ok", SqlDbType.Bit).Value = succeeded;
        command.Parameters.Add("@details", SqlDbType.NVarChar, 300).Value = details is null ? DBNull.Value : details;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Opretter en ordre og opdaterer lageret samlet i én transaktion.</summary>
    public async Task<int> CreateOrderAsync(int userId, IReadOnlyList<CartLine> cart)
    {
        if (cart.Count == 0) throw new InvalidOperationException("Kurven er tom.");
        await using var connection = factory.Create();
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            // Produkterne låses under kontrollen, så lageret ikke ændres midt i købet.
            var pricedLines = new List<(Product Product, int Quantity)>();
            foreach (var line in cart)
            {
                if (line.Quantity is < 1 or > 20) throw new InvalidOperationException("Antallet skal være mellem 1 og 20.");
                const string productSql = "SELECT Id, Name, Description, Price, Stock, IsActive FROM Product WITH (UPDLOCK, ROWLOCK) WHERE Id = @id AND IsActive = 1;";
                await using var productCommand = new SqlCommand(productSql, connection, transaction);
                productCommand.Parameters.Add("@id", SqlDbType.Int).Value = line.ProductId;
                await using var reader = await productCommand.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) throw new InvalidOperationException("Et produkt findes ikke længere.");
                var product = ReadProduct(reader);
                await reader.CloseAsync();
                if (product.Stock < line.Quantity) throw new InvalidOperationException($"Der er ikke nok på lager af {product.Name}.");
                pricedLines.Add((product, line.Quantity));
            }

            var total = pricedLines.Sum(line => line.Product.Price * line.Quantity);
            await using var orderCommand = new SqlCommand("INSERT INTO ShopOrder (UserId, Total) OUTPUT INSERTED.Id VALUES (@userId, @total);", connection, transaction);
            orderCommand.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
            orderCommand.Parameters.Add("@total", SqlDbType.Decimal).Value = total;
            orderCommand.Parameters["@total"].Precision = 18;
            orderCommand.Parameters["@total"].Scale = 2;
            var orderId = (int)(await orderCommand.ExecuteScalarAsync() ?? throw new InvalidOperationException("Ordren blev ikke oprettet."));

            foreach (var line in pricedLines)
            {
                const string lineSql = """
                    INSERT INTO OrderLine (OrderId, ProductId, ProductName, UnitPrice, Quantity)
                    VALUES (@orderId, @productId, @name, @price, @quantity);
                    UPDATE Product SET Stock = Stock - @quantity WHERE Id = @productId AND Stock >= @quantity;
                    """;
                await using var lineCommand = new SqlCommand(lineSql, connection, transaction);
                lineCommand.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                lineCommand.Parameters.Add("@productId", SqlDbType.Int).Value = line.Product.Id;
                lineCommand.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = line.Product.Name;
                var price = lineCommand.Parameters.Add("@price", SqlDbType.Decimal);
                price.Precision = 18; price.Scale = 2; price.Value = line.Product.Price;
                lineCommand.Parameters.Add("@quantity", SqlDbType.Int).Value = line.Quantity;
                if (await lineCommand.ExecuteNonQueryAsync() < 2) throw new InvalidOperationException("Lageret blev ændret. Prøv igen.");
            }
            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>Henter en brugers ordrer med de nyeste først.</summary>
    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(int userId)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT Id, CreatedUtc, Total FROM ShopOrder WHERE UserId = @userId ORDER BY CreatedUtc DESC;", connection);
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
        var orders = new List<OrderSummary>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) orders.Add(new(reader.GetInt32(0), reader.GetDateTimeOffset(1), reader.GetDecimal(2)));
        return orders;
    }

    /// <summary>Henter en ordre, hvis den tilhører den angivne bruger.</summary>
    public async Task<OrderDetails?> GetOrderAsync(int userId, int orderId)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = """
            SELECT o.Id, o.CreatedUtc, o.Total, l.ProductName, l.UnitPrice, l.Quantity
            FROM ShopOrder o JOIN OrderLine l ON l.OrderId = o.Id
            WHERE o.Id = @orderId AND o.UserId = @userId ORDER BY l.Id;
            """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
        await using var reader = await command.ExecuteReaderAsync();
        var lines = new List<OrderLineView>();
        int id = 0; DateTimeOffset created = default; decimal total = 0;
        while (await reader.ReadAsync())
        {
            if (id == 0) { id = reader.GetInt32(0); created = reader.GetDateTimeOffset(1); total = reader.GetDecimal(2); }
            lines.Add(new(reader.GetString(3), reader.GetDecimal(4), reader.GetInt32(5)));
        }
        return id == 0 ? null : new(id, created, total, lines);
    }

    /// <summary>Opretter et nyt produkt eller opdaterer et eksisterende.</summary>
    public async Task SaveProductAsync(Product product)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        const string sql = """
            IF @id = 0
              INSERT INTO Product (Name, Description, Price, Stock, IsActive) VALUES (@name, @description, @price, @stock, 1);
            ELSE
              UPDATE Product SET Name=@name, Description=@description, Price=@price, Stock=@stock WHERE Id=@id;
            """;
        await using var command = new SqlCommand(sql, connection);
        AddProductParameters(command, product);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Skjuler et produkt uden at slette tidligere ordredata.</summary>
    public async Task DeactivateProductAsync(int id)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        await using var command = new SqlCommand("UPDATE Product SET IsActive = 0 WHERE Id = @id;", connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Udfører en enkel opdatering på én bruger.</summary>
    private async Task ExecuteUserUpdateAsync(string sql, int userId)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Omsætter den aktuelle databaserække til et produkt.</summary>
    private static Product ReadProduct(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0), Name = reader.GetString(1), Description = reader.GetString(2),
        Price = reader.GetDecimal(3), Stock = reader.GetInt32(4), IsActive = reader.GetBoolean(5)
    };

    /// <summary>Omsætter den aktuelle databaserække til en bruger.</summary>
    private static AppUser ReadUser(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0), Email = reader.GetString(1), PasswordHash = reader.GetString(2), Role = reader.GetString(3),
        FailedLoginCount = reader.GetInt32(4), LockedUntilUtc = reader.IsDBNull(5) ? null : reader.GetDateTimeOffset(5)
    };

    /// <summary>Tilføjer produktværdier som sikre SQL-parametre.</summary>
    private static void AddProductParameters(SqlCommand command, Product product)
    {
        command.Parameters.Add("@id", SqlDbType.Int).Value = product.Id;
        command.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = product.Name;
        command.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = product.Description;
        var price = command.Parameters.Add("@price", SqlDbType.Decimal); price.Precision = 18; price.Scale = 2; price.Value = product.Price;
        command.Parameters.Add("@stock", SqlDbType.Int).Value = product.Stock;
    }
}
