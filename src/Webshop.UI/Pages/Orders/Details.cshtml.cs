using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Security;

namespace Webshop.UI.Pages.Orders;

/// <summary>Viser detaljerne for én af den indloggede brugers ordrer.</summary>
public sealed class DetailsModel(StoreRepository repository) : PageModel
{
    public OrderDetails Order { get; private set; } = null!;
    /// <summary>Henter ordren og afviser opslag på andre brugeres ordrer.</summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var order = await repository.GetOrderAsync(User.UserId(), id);
        if (order is null) { await repository.WriteAuditAsync(User.UserId(), "OrderAccessDenied", false); return NotFound(); }
        Order = order; return Page();
    }
}
