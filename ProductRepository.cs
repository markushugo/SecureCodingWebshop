using System.Collections.Generic;
using System.Data.SqlClient;
using Webshop.Library.Models;
using Webshop.Library.Repositories;

namespace Webshop.Library.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Product> GetActiveProducts()
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ProductId, Name, Description, Price FROM Products WHERE IsActive = 1", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                                ? null 
                                : reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = true
                        });
                    }
                }
            }

            return products;
        }

        public IEnumerable<Product> SearchProducts(string query)
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ProductId, Name, Description, Price FROM Products WHERE IsActive = 1 AND (Name LIKE @query OR Description LIKE @query)", connection);
                command.Parameters.AddWithValue("@query", "%" + query + "%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                                ? null 
                                : reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = true
                        });
                    }
                }
            }

            return products;
        }
    }
}