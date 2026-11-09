using Webshop.Library.Models;

namespace Webshop.Library.Services
{
    public class CartService
    {
        private readonly List<CartItem> _cart = new List<CartItem>();

        public void AddToCart(Product product, int quantity)
        {
            var existingItem = _cart.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cart.Add(new CartItem { Product = product, Quantity = quantity });
            }
        }

        public void RemoveFromCart(int productId)
        {
            var item = _cart.FirstOrDefault(item => item.Product.Id == productId);
            if (item != null)
            {
                _cart.Remove(item);
            }
        }

        public List<CartItem> GetCartItems()
        {
            return _cart;
        }

        public decimal CalculateTotal()
        {
            return _cart.Sum(item => item.Product.Price * item.Quantity);
        }
    }
}