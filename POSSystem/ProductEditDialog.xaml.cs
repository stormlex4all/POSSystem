using System.Windows;
using System.Windows.Controls;
using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem
{
    public partial class ProductEditDialog : Window
    {
        private readonly IProductService _productService;
        private Product _product;
        private bool _isEditMode;

        // Constructor for adding new product
        public ProductEditDialog(IProductService productService)
        {
            InitializeComponent();
            _productService = productService;
            _isEditMode = false;
            HeaderText.Text = "ADD NEW PRODUCT";
            CategoryComboBox.SelectedIndex = 0;
        }

        // Constructor for editing existing product
        public ProductEditDialog(IProductService productService, Product product)
        {
            InitializeComponent();
            _productService = productService;
            _product = product;
            _isEditMode = true;
            HeaderText.Text = "EDIT PRODUCT";
            LoadProduct(product);
        }

        private void LoadProduct(Product product)
        {
            NameTextBox.Text = product.Name;
            PriceTextBox.Text = product.Price.ToString("F2");
            StockTextBox.Text = product.Stock.ToString();
            BarcodeTextBox.Text = product.Barcode;
            AgeVerificationCheckBox.IsChecked = product.RequiresAgeVerification;

            // Set category
            foreach (ComboBoxItem item in CategoryComboBox.Items)
            {
                if (item.Content.ToString() == product.Category)
                {
                    CategoryComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Product name is required.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid price (0 or greater).", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(StockTextBox.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Please enter a valid stock quantity (0 or greater).", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Misc";

                if (_isEditMode)
                {
                    // Update existing product
                    _product.Name = NameTextBox.Text;
                    _product.Price = price;
                    _product.Stock = stock;
                    _product.Category = category;
                    _product.Barcode = BarcodeTextBox.Text;
                    _product.RequiresAgeVerification = AgeVerificationCheckBox.IsChecked == true;

                    _productService.UpdateProduct(_product);
                    MessageBox.Show("Product updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Create new product
                    var newProduct = new Product
                    {
                        Name = NameTextBox.Text,
                        Price = price,
                        Stock = stock,
                        Category = category,
                        Barcode = BarcodeTextBox.Text,
                        RequiresAgeVerification = AgeVerificationCheckBox.IsChecked == true
                    };

                    _productService.AddProduct(newProduct);
                    MessageBox.Show("Product added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
