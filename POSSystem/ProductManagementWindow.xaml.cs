using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem
{
    public partial class ProductManagementWindow : Window
    {
        private readonly IProductService _productService;

        public ProductManagementWindow(IProductService productService)
        {
            _productService = productService;
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            if (ProductsDataGrid != null)
            {
                ProductsDataGrid.ItemsSource = _productService.GetAllProducts();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductsDataGrid != null && SearchTextBox != null)
            {
                var searchTerm = SearchTextBox.Text;
                ProductsDataGrid.ItemsSource = _productService.SearchProducts(searchTerm);
            }
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilterComboBox?.SelectedItem is ComboBoxItem selected && ProductsDataGrid != null)
            {
                var category = selected.Content.ToString();

                if (category == "All Categories")
                {
                    LoadProducts();
                }
                else
                {
                    ProductsDataGrid.ItemsSource = _productService.GetProductsByCategory(category);
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductEditDialog(_productService);
            if (dialog.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var dialog = new ProductEditDialog(_productService, selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedProduct.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _productService.DeleteProduct(selectedProduct.Id);
                    LoadProducts();
                    MessageBox.Show("Product deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            CategoryFilterComboBox.SelectedIndex = 0;
            LoadProducts();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
