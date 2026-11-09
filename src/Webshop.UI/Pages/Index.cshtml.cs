using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Services;

namespace Webshop.UI.Pages;

/// <summary>Viser produktkataloget og håndterer søgning og tilføjelse til kurven.</summary>
public sealed class IndexModel(StoreRepository repository, CartSession cart) : PageModel
{
    public IReadOnlyList<Product> Products { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    /// <summary>Henter de produkter, der passer til den valgte søgning.</summary>
    public async Task OnGetAsync()
    {
        if (Search?.Length > 60) ModelState.AddModelError(nameof(Search), "Søgningen må højst være 60 tegn.");
        Products = ModelState.IsValid ? await repository.GetProductsAsync(Search) : [];
    }

    /// <summary>Kontrollerer produktet og lægger det i kurven.</summary>
    public async Task<IActionResult> OnPostAddAsync(int productId)
    {
        if (await repository.GetProductAsync(productId) is null) return NotFound();
        cart.Add(productId);
        TempData["Message"] = "Produktet er lagt i kurven.";
        return RedirectToPage();
    }
}
