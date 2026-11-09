using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Models;
using Webshop.UI.Services;

namespace Webshop.UI.Pages.Cart;

/// <summary>Viser kurven og håndterer ændringer af dens indhold.</summary>
public sealed class IndexModel(CartSession cart) : PageModel
{
    public IReadOnlyList<CartDisplayLine> Lines { get; private set; } = [];
    public decimal Total => Lines.Sum(line => line.LineTotal);
    /// <summary>Henter kurvens aktuelle indhold til siden.</summary>
    public async Task OnGetAsync() => Lines = await cart.DisplayAsync();

    /// <summary>Opdaterer antallet for et produkt i kurven.</summary>
    public IActionResult OnPostUpdate(int productId, int quantity) { cart.Update(productId, quantity); return RedirectToPage(); }

    /// <summary>Fjerner et produkt fra kurven.</summary>
    public IActionResult OnPostRemove(int productId) { cart.Remove(productId); return RedirectToPage(); }

    /// <summary>Tømmer hele kurven.</summary>
    public IActionResult OnPostClear() { cart.Clear(); return RedirectToPage(); }
}
