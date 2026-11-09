using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.UI.Security;

namespace Webshop.UI.Pages.Admin.Products;

/// <summary>Håndterer oprettelse og redigering af produkter.</summary>
public sealed class EditModel(StoreRepository repository) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public sealed class InputModel
    {
        public int Id { get; set; }
        [Required, StringLength(120)] public string Name { get; set; } = "";
        [Required, StringLength(1000)] public string Description { get; set; } = "";
        [Range(0.01, 1_000_000)] public decimal Price { get; set; }
        [Range(0, 1_000_000)] public int Stock { get; set; }
    }
    /// <summary>Henter et eksisterende produkt eller viser en tom formular.</summary>
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null) return Page();
        var product = await repository.GetProductAsync(id.Value, true);
        if (product is null) return NotFound();
        Input = new() { Id=product.Id, Name=product.Name, Description=product.Description, Price=product.Price, Stock=product.Stock };
        return Page();
    }
    /// <summary>Validerer og gemmer produktets oplysninger.</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await repository.SaveProductAsync(new Product { Id=Input.Id, Name=Input.Name.Trim(), Description=Input.Description.Trim(), Price=Input.Price, Stock=Input.Stock, IsActive=true });
        await repository.WriteAuditAsync(User.UserId(), Input.Id == 0 ? "ProductCreated" : "ProductUpdated", true);
        return RedirectToPage("Index");
    }
}
