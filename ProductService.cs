using System.Collections.Generic;
using Webshop.Library.Models;
using Webshop.Library.Repositories;
using Webshop.Library.Services;

namespace Webshop.Library.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IEnumerable<Product> GetActiveProducts()
        {
            return _productRepository.GetActiveProducts();
        }

        public IEnumerable<Product> SearchProducts(string query)
        {
            return _productRepository.SearchProducts(query);
        }
    }
}