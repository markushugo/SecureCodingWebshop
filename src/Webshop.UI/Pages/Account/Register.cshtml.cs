using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Services;

namespace Webshop.UI.Pages.Account;

/// <summary>Håndterer oprettelse af en ny kundekonto.</summary>
public sealed class RegisterModel(UserService users) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public int? PasswordScore { get; private set; }
    public sealed class InputModel
    {
        [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>Sender allerede indloggede brugere tilbage til forsiden.</summary>
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

    /// <summary>Validerer formularen og forsøger at oprette brugeren.</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrEmpty(Input.Password)) PasswordScore = PasswordPolicy.Score(Input.Password);
        if (!ModelState.IsValid) return Page();
        foreach (var error in await users.RegisterAsync(Input.Email, Input.Password)) ModelState.AddModelError(string.Empty, error);
        if (!ModelState.IsValid) return Page();
        TempData["Message"] = "Brugeren er oprettet. Du kan nu logge ind.";
        return RedirectToPage("Login");
    }
}
