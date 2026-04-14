using POSSystem.Models;
using POSSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace POSSystem
{
    public partial class MainWindow : Window
    {
        private readonly IProductService _productService;
        public ObservableCollection<CartItem> Cart { get; set; }
        private string currentInput = "";
        private int pendingQuantity = 1;
        private string currentCategory = "";
        private string timeFormat = "dddd, MMMM dd, yyyy - hh:mm:ss tt";

        public MainWindow()
        {
            InitializeComponent();

            _productService = new ProductService();

            Cart = new ObservableCollection<CartItem>();
            DataContext = this;

            LoadProductButtons();
            UpdateTotal();
            UpdateEmptyMessage();

            // Start timer for date/time display
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) => DateTimeTextBlock.Text = DateTime.Now.ToString(timeFormat);
            timer.Start();
            DateTimeTextBlock.Text = DateTime.Now.ToString(timeFormat);
        }

        private void LoadProductButtons(string category = "")
        {
            ProductsPanel.Children.Clear();

            var products = string.IsNullOrEmpty(category)
                ? _productService.GetAllProducts()
                : _productService.GetProductsByCategory(category);

            foreach (var product in products)
            {
                var button = new Button
                {
                    Content = $"{product.Name}\n${product.Price:F2}",
                    Width = 120,
                    Height = 80,
                    Margin = new Thickness(5),
                    Tag = product,
                    Background = product.RequiresAgeVerification
                        ? new SolidColorBrush(Color.FromRgb(255, 152, 0)) // Orange for 18+
                        : new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue for regular
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                };

                button.Click += Product_Click;
                ProductsPanel.Children.Add(button);
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CartItem item)
            {
                int amount = pendingQuantity;

                if (item.Quantity > amount)
                {
                    item.Quantity -= amount;
                }
                else
                {
                    Cart.Remove(item);
                }

                pendingQuantity = 1;
                UpdateTotal();
                UpdateEmptyMessage();
                ResetInput();
            }
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CartItem item)
            {
                int amount = pendingQuantity;
                item.Quantity += amount;

                pendingQuantity = 1;
                UpdateTotal();
                ResetInput();
            }
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            string value = (sender as Button).Content.ToString();

            if (value == "." && currentInput.Contains("."))
                return;

            if (value == "." && string.IsNullOrEmpty(currentInput))
            {
                currentInput = "0.";
            }

            if (string.IsNullOrEmpty(currentInput))
                currentInput = value;
            else
                currentInput += value;

            QuantityDisplay.Text = currentInput;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            currentInput = "";
            QuantityDisplay.Text = "";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentInput))
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }

            QuantityDisplay.Text = currentInput;
        }

        private void Quantity_Click(object sender, RoutedEventArgs e)
        {
            pendingQuantity = int.TryParse(currentInput, out int result) ? result : 1;
            ResetInput();
            QuantityDisplay.Text = $"⚠ Adjusting by: {pendingQuantity}";
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Product product = btn.Tag as Product;

            if (product == null) return;

            // Check stock availability
            int qty = pendingQuantity;
            if (!_productService.IsProductInStock(product.Id, qty))
            {
                MessageBox.Show($"Insufficient stock for {product.Name}. Only {product.Stock} available.",
                    "Stock Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Age verification check
            if (product.RequiresAgeVerification)
            {
                var ageDialog = new AgeVerificationDialog();
                if (ageDialog.ShowDialog() != true)
                {
                    MessageBox.Show("Age verification failed. Cannot add this item.",
                        "Age Verification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Add to cart
            var existingItem = Cart.FirstOrDefault(x => x.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                Cart.Add(new CartItem(product, qty));
            }

            pendingQuantity = 1;
            UpdateTotal();
            UpdateEmptyMessage();
            ResetInput();
        }

        private void ResetInput()
        {
            currentInput = "";
            QuantityDisplay.Text = "";
        }

        private void UpdateTotal()
        {
            double total = Cart.Sum(item => item.Subtotal);
            TotalText.Text = $"Total: {total:C}";
        }

        private void UpdateEmptyMessage()
        {
            if (Cart.Count == 0)
                EmptyMessage.Visibility = Visibility.Visible;
            else
                EmptyMessage.Visibility = Visibility.Collapsed;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Cancel Transaction?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Cart.Clear();
                UpdateTotal();
                UpdateEmptyMessage();
            }
        }

        private void Hold_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Nothing to hold.", "Empty Cart",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Transaction Held (Feature to be fully implemented)",
                "Hold Transaction", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double total = Cart.Sum(x => x.Subtotal);
            double cad = total * 1.38; // USD to CAD conversion

            MessageBox.Show($"USD: ${total:F2}\nCAD: ${cad:F2}",
                "Currency Conversion", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add items before proceeding to payment.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Card/Other Payment (Feature to be fully implemented)",
                "Payment", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add items before proceeding to payment.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double total = Cart.Sum(x => x.Subtotal);

            var payment = new PaymentWindow(total, () =>
            {
                // Update product stock after successful payment
                foreach (var item in Cart)
                {
                    item.Product.UpdateStock(-item.Quantity);
                }

                Cart.Clear();
                UpdateTotal();
                UpdateEmptyMessage();
            });

            payment.ShowDialog();
        }

        private void Receipt_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Nothing to show on receipt.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var receipt = new ReceiptWindow(Cart);
            receipt.ShowDialog();
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string category = btn.Tag?.ToString();

            if (!string.IsNullOrEmpty(category))
            {
                currentCategory = category;
                LoadProductButtons(category);
            }
        }

        private void ProductManagement_Click(object sender, RoutedEventArgs e)
        {
            var productMgmtWindow = new ProductManagementWindow(_productService);
            productMgmtWindow.ShowDialog();

            // Refresh products after management window closes
            LoadProductButtons(currentCategory);
        }
    }
}