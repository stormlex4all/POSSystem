using System.Collections.ObjectModel;
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
    }

    // Product service implementation
    public class ProductService : IProductService
    {
        private ObservableCollection<Product> _products;

        public ProductService()
        {
            _products = new ObservableCollection<Product>();
            InitializeSampleProducts();
        }

        private void InitializeSampleProducts()
        {
            // Food items
            _products.Add(new Product("Sandwich", 5.99, 20, "Food", false));
            _products.Add(new Product("Hot Dog", 3.49, 25, "Food", false));
            _products.Add(new Product("Pizza Slice", 4.99, 15, "Food", false));

            // Beverages
            _products.Add(new Product("Coke", 1.99, 50, "Beverages", false));
            _products.Add(new Product("Pepsi", 1.99, 50, "Beverages", false));
            _products.Add(new Product("Water", 1.49, 100, "Beverages", false));
            _products.Add(new Product("Energy Drink", 3.99, 30, "Beverages", false));

            // Snacks
            _products.Add(new Product("Chips", 2.49, 40, "Snacks", false));
            _products.Add(new Product("Chocolate", 1.99, 60, "Snacks", false));
            _products.Add(new Product("Candy Bar", 1.49, 75, "Snacks", false));
            _products.Add(new Product("Gum", 1.99, 50, "Snacks", false));

            // Age-restricted items
            _products.Add(new Product("Cigarettes", 15.99, 20, "18+", true) { Barcode = "123456789" });
            _products.Add(new Product("Tobacco", 12.99, 15, "18+", true) { Barcode = "987654321" });

            // Miscellaneous
            _products.Add(new Product("Magazine", 4.99, 30, "Misc", false));
            _products.Add(new Product("Batteries", 6.99, 25, "Misc", false));
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
        }

        public void DeleteProduct(string productId)
        {
            var product = GetProductById(productId);
            if (product != null)
            {
                _products.Remove(product);
            }
        }

        public Product GetProductById(string productId)
        {
            return _products.FirstOrDefault(p => p.Id == productId);
        }

        public ObservableCollection<Product> GetAllProducts()
        {
            return _products;
        }

        public ObservableCollection<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _products;

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
                return _products;

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
    }
}
