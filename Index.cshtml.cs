using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Webshop.Library.Models;
using Webshop.Library.Services;

namespace Webshop.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IEnumerable<Product> Products { get; set; }

        public void OnGet()
        {
            Products = _productService.GetActiveProducts();
        }
    }
}