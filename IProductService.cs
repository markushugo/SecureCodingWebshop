using System.Collections.Generic;
using Webshop.Library.Models;

namespace Webshop.Library.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetActiveProducts();
        IEnumerable<Product> SearchProducts(string query);
    }
}