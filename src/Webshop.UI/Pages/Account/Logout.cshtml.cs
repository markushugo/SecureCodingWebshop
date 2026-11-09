using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.UI.Security;

namespace Webshop.UI.Pages.Account;

/// <summary>Logger brugeren sikkert ud af webshoppen.</summary>
public sealed class LogoutModel(StoreRepository repository) : PageModel
{
    /// <summary>Registrerer udlogningen og fjerner login-cookien.</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true) await repository.WriteAuditAsync(User.UserId(), "Logout", true);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }
}
