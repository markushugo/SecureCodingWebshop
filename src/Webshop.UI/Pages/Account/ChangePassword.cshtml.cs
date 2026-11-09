using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Services;

namespace Webshop.UI.Pages.Account;

/// <summary>Håndterer ændring af den indloggede brugers adgangskode.</summary>
public sealed class ChangePasswordModel(UserService users) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public int? PasswordScore { get; private set; }
    public sealed class InputModel
    {
        [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
        [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = "";
        [Required, DataType(DataType.Password), Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = "";
    }
    /// <summary>Validerer formularen og ændrer adgangskoden, hvis oplysningerne er korrekte.</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrEmpty(Input.NewPassword)) PasswordScore = PasswordPolicy.Score(Input.NewPassword);
        if (!ModelState.IsValid) return Page();
        var user = await users.FindAsync(User.Identity!.Name!);
        if (user is null) return Challenge();
        foreach (var error in await users.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword)) ModelState.AddModelError(string.Empty, error);
        if (!ModelState.IsValid) return Page();
        TempData["Message"] = "Adgangskoden er ændret.";
        return RedirectToPage("/Index");
    }
}
