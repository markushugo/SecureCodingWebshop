using System.Collections.Generic;
using Webshop.Library.Models;

namespace Webshop.Library.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetActiveProducts();
        IEnumerable<Product> SearchProducts(string query);
    }
}