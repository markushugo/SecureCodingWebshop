using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Security;

namespace Webshop.UI.Pages.Orders;

/// <summary>Viser den indloggede brugers ordrehistorik.</summary>
public sealed class IndexModel(StoreRepository repository) : PageModel
{
    public IReadOnlyList<OrderSummary> Orders { get; private set; } = [];
    /// <summary>Henter kun de ordrer, der tilhører den aktuelle bruger.</summary>
    public async Task OnGetAsync() => Orders = await repository.GetOrdersAsync(User.UserId());
}
