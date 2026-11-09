using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Security;

namespace Webshop.UI.Pages.Admin.Products;

/// <summary>Viser administratorens oversigt over alle produkter.</summary>
public sealed class IndexModel(StoreRepository repository) : PageModel
{
    public IReadOnlyList<Product> Products { get; private set; } = [];
    /// <summary>Henter både aktive og deaktiverede produkter.</summary>
    public async Task OnGetAsync() => Products = await repository.GetProductsAsync(null, true);

    /// <summary>Deaktiverer et produkt og registrerer handlingen i auditloggen.</summary>
    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        await repository.DeactivateProductAsync(id);
        await repository.WriteAuditAsync(User.UserId(), "ProductDeactivated", true, $"ProductId={id}");
        return RedirectToPage();
    }
}
