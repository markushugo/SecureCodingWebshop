namespace Webshop.Library.Models;

/// <summary>Viser de vigtigste oplysninger om en ordre i ordreoversigten.</summary>
public sealed record OrderSummary(int Id, DateTimeOffset CreatedUtc, decimal Total);
/// <summary>Viser én varelinje på en ordre.</summary>
public sealed record OrderLineView(string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
/// <summary>Indeholder en samlet ordre og alle dens varelinjer.</summary>
public sealed record OrderDetails(int Id, DateTimeOffset CreatedUtc, decimal Total, IReadOnlyList<OrderLineView> Lines);
