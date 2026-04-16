using System.Collections.ObjectModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Services
{
    // Interface for product service - demonstrates abstraction
    public interface IProductService
    {
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(string productId);
        Product GetProductById(string productId);
        ObservableCollection<Product> GetAllProducts();
        ObservableCollection<Product> SearchProducts(string searchTerm);
        ObservableCollection<Product> GetProductsByCategory(string category);
        bool IsProductInStock(string productId, int requestedQuantity);
        void SaveChanges();
    }

    // Product service implementation
    public class ProductService : IProductService
    {
        private readonly LocalView<Product> _products;
        private readonly POSDbContext _dbContext;


        public ProductService(POSDbContext context)
        {
            _dbContext = context;
            _dbContext.Products.Load();
            _products = _dbContext.Products.Local;
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required");

            if (product.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            _products.Add(product);
            _dbContext.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var existing = GetProductById(product.Id);
            if (existing == null)
                throw new InvalidOperationException("Product not found");

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.Category = product.Category;
            existing.RequiresAgeVerification = product.RequiresAgeVerification;
            existing.Barcode = product.Barcode;
            _dbContext.SaveChanges();
        }

        public void DeleteProduct(string productId)
        {
            var product = GetProductById(productId);
            if (product != null)
            {
                _products.Remove(product);
            }
            _dbContext.SaveChanges();
        }

        public Product GetProductById(string productId)
        {
            return _products.FirstOrDefault(p => p.Id == productId);
        }

        public ObservableCollection<Product> GetAllProducts()
        {
            return _products.ToObservableCollection();
        }

        public ObservableCollection<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _products.ToObservableCollection();

            var filtered = _products.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm))
            ).ToList();

            return new ObservableCollection<Product>(filtered);
        }

        public ObservableCollection<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return _products.ToObservableCollection();

            var filtered = _products.Where(p =>
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            return new ObservableCollection<Product>(filtered);
        }

        public bool IsProductInStock(string productId, int requestedQuantity)
        {
            var product = GetProductById(productId);
            return product != null && product.Stock >= requestedQuantity;
        }

        public void SaveChanges() => _dbContext.SaveChanges();
    }
}
