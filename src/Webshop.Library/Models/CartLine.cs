namespace Webshop.Library.Models;

/// <summary>Gemmer et produkt og det valgte antal i kurven.</summary>
public sealed class CartLine
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>Indeholder de produktdata, der vises på kurvesiden.</summary>
public sealed record CartDisplayLine(int ProductId, string Name, decimal UnitPrice, int Quantity, int AvailableStock)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
