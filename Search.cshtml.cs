using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Webshop.Library.Models;
using Webshop.Library.Services;

namespace Webshop.UI.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IProductService _productService;

        public SearchModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(Query))
            {
                Products = _productService.SearchProducts(Query);
            }
            else
            {
                Products = new List<Product>();
            }
        }
    }
}