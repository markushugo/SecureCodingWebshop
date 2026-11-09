using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Security;
using Webshop.UI.Services;

namespace Webshop.UI.Pages.Checkout;

/// <summary>Viser checkout og gennemfører et simuleret køb.</summary>
public sealed class IndexModel(CartSession cart, StoreRepository repository) : PageModel
{
    public IReadOnlyList<CartDisplayLine> Lines { get; private set; } = [];
    public decimal Total => Lines.Sum(line => line.LineTotal);
    /// <summary>Henter kurvens indhold til checkout-siden.</summary>
    public async Task OnGetAsync() => Lines = await cart.DisplayAsync();

    /// <summary>Opretter ordren og tømmer kurven, når købet lykkes.</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.UserId();
        try
        {
            var orderId = await repository.CreateOrderAsync(userId, cart.Get());
            cart.Clear();
            await repository.WriteAuditAsync(userId, "Checkout", true, $"OrderId={orderId}");
            TempData["Message"] = "Købet er gennemført som lokal simulation.";
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }
        catch (InvalidOperationException error)
        {
            await repository.WriteAuditAsync(userId, "Checkout", false, "Validering afviste checkout");
            ModelState.AddModelError(string.Empty, error.Message);
            Lines = await cart.DisplayAsync();
            return Page();
        }
    }
}
