namespace Webshop.Library.Models;

/// <summary>Beskriver et produkt, der kan vises og sælges i webshoppen.</summary>
public sealed class Product
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public bool IsActive { get; init; }
}
