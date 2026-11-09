using System.Text.Json;
using Webshop.Library.Data;
using Webshop.Library.Models;

namespace Webshop.UI.Services;

/// <summary>Gemmer og vedligeholder indkøbskurven i brugerens session.</summary>
public sealed class CartSession(IHttpContextAccessor accessor, StoreRepository repository)
{
    private const string Key = "Cart";
    private ISession Session => accessor.HttpContext?.Session ?? throw new InvalidOperationException("Session er ikke tilgængelig.");

    /// <summary>Henter den aktuelle kurv fra sessionen.</summary>
    public IReadOnlyList<CartLine> Get() => JsonSerializer.Deserialize<List<CartLine>>(Session.GetString(Key) ?? "[]") ?? [];

    /// <summary>Tilføjer et produkt eller øger antallet, hvis det allerede er i kurven.</summary>
    public void Add(int productId, int quantity = 1)
    {
        var cart = Get().ToList();
        var line = cart.SingleOrDefault(item => item.ProductId == productId);
        if (line is null) cart.Add(new CartLine { ProductId = productId, Quantity = Math.Clamp(quantity, 1, 20) });
        else line.Quantity = Math.Clamp(line.Quantity + quantity, 1, 20);
        Save(cart);
    }

    /// <summary>Ændrer antallet eller fjerner produktet, hvis antallet er nul.</summary>
    public void Update(int productId, int quantity)
    {
        var cart = Get().ToList();
        var line = cart.SingleOrDefault(item => item.ProductId == productId);
        if (line is null) return;
        if (quantity <= 0) cart.Remove(line); else line.Quantity = Math.Clamp(quantity, 1, 20);
        Save(cart);
    }

    /// <summary>Fjerner et bestemt produkt fra kurven.</summary>
    public void Remove(int productId) => Save(Get().Where(item => item.ProductId != productId).ToList());

    /// <summary>Tømmer hele kurven.</summary>
    public void Clear() => Session.Remove(Key);

    /// <summary>Henter de aktuelle produktdata til visning af kurven.</summary>
    public async Task<IReadOnlyList<CartDisplayLine>> DisplayAsync()
    {
        var result = new List<CartDisplayLine>();
        foreach (var line in Get())
        {
            var product = await repository.GetProductAsync(line.ProductId);
            if (product is not null) result.Add(new(product.Id, product.Name, product.Price, line.Quantity, product.Stock));
        }
        return result;
    }

    /// <summary>Gemmer kurven som JSON i brugerens session.</summary>
    private void Save(IReadOnlyList<CartLine> cart) => Session.SetString(Key, JsonSerializer.Serialize(cart));
}
