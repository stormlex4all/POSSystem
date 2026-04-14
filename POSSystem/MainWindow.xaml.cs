using POSSystem.Models;
using POSSystem.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Transaction = POSSystem.Models.Transaction;

namespace POSSystem
{
    public partial class MainWindow : Window
    {
        private readonly IProductService _productService;
        private readonly ITransactionService _transactionService;
        
        private Transaction _currentTransaction;
        public ObservableCollection<CartItem> Cart { get; set; }
        private string currentInput = "";
        private int pendingQuantity = 1;
        private string currentCategory = "";
        private string timeFormat = "dddd, MMMM dd, yyyy - hh:mm:ss tt";

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            _productService = new ProductService();
            _transactionService = new TransactionService();

            // Create initial transaction
            _currentTransaction = _transactionService.CreateTransaction();
            Cart = _currentTransaction.Cart;

            DataContext = this;

            LoadProductButtons();
            UpdateTotal();
            UpdateEmptyMessage();
            UpdateHeldTransactionCount();

            // Start timer for date/time display
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) => DateTimeTextBlock.Text = DateTime.Now.ToString(timeFormat);
            timer.Start();
            DateTimeTextBlock.Text = DateTime.Now.ToString(timeFormat);
        }

        public static List<Transaction> TransactionHistory = new List<Transaction>();

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
                    _currentTransaction.Cart.Remove(item);
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

            // Age verification check - ONCE PER TRANSACTION
            if (product.RequiresAgeVerification && !_currentTransaction.AgeVerified)
            {
                var ageDialog = new AgeVerificationDialog();
                if (ageDialog.ShowDialog() == true)
                {
                    // Mark transaction as age-verified (applies to all future age-restricted items)
                    _currentTransaction.AgeVerified = true;
                }
                else
                {
                    MessageBox.Show("Age verification failed. Cannot add age-restricted items to this transaction.",
                        "Age Verification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Add to cart
            var existingItem = _currentTransaction.Cart.FirstOrDefault(x => x.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                _currentTransaction.Cart.Add(new CartItem(product, qty));
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
            _currentTransaction.CalculateTotals();
            TotalText.Text = $"Total: {_currentTransaction.Total:C}";
        }

        private void UpdateEmptyMessage()
        {
            if (_currentTransaction.Cart.Count == 0)
                EmptyMessage.Visibility = Visibility.Visible;
            else
                EmptyMessage.Visibility = Visibility.Collapsed;
        }

        private void UpdateHeldTransactionCount()
        {
            int heldCount = _transactionService.GetHeldTransactionCount();
            HeldCountTextBlock.Text = heldCount.ToString();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTransaction.Cart.Count == 0)
            {
                MessageBox.Show("Transaction is already empty.", "Nothing to Abort",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to abort this transaction?\n\nAll items will be removed and the transaction will be discarded.",
                "Confirm Abort Transaction",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _transactionService.AbortTransaction(_currentTransaction);
                
                // Create new transaction
                _currentTransaction = _transactionService.CreateTransaction();
                CartGrid.ItemsSource = _currentTransaction.Cart;
                
                UpdateTotal();
                UpdateEmptyMessage();
                
                MessageBox.Show("Transaction aborted successfully.", "Transaction Aborted",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Hold_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTransaction.Cart.Count == 0)
            {
                MessageBox.Show("Cannot hold an empty transaction.", "Empty Cart",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _transactionService.HoldTransaction(_currentTransaction);
                
                MessageBox.Show(
                    $"Transaction held successfully!\n\n" +
                    $"Transaction ID: {_currentTransaction.Id.Substring(0, 8)}...\n" +
                    $"Items: {_currentTransaction.Cart.Count}\n" +
                    $"Total: {_currentTransaction.Total:C}\n\n" +
                    $"You can resume this transaction later.",
                    "Transaction Held",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Create new transaction
                _currentTransaction = _transactionService.CreateTransaction();
                CartGrid.ItemsSource = _currentTransaction.Cart;
                
                UpdateTotal();
                UpdateEmptyMessage();
                UpdateHeldTransactionCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error holding transaction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            var heldTransactions = _transactionService.GetHeldTransactions();
            
            if (heldTransactions.Count == 0)
            {
                MessageBox.Show("No held transactions available.", "No Held Transactions",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check if current transaction has items
            if (_currentTransaction.Cart.Count > 0)
            {
                var result = MessageBox.Show(
                    "Current transaction has items. Do you want to hold it before resuming another?\n\n" +
                    "Yes - Hold current and resume selected\n" +
                    "No - Discard current and resume selected\n" +
                    "Cancel - Keep working on current",
                    "Current Transaction",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes)
                {
                    _transactionService.HoldTransaction(_currentTransaction);
                }
                else
                {
                    _transactionService.AbortTransaction(_currentTransaction);
                }
            }

            // Show held transactions dialog
            var dialog = new HeldTransactionsDialog(_transactionService);
            if (dialog.ShowDialog() == true && dialog.SelectedTransaction != null)
            {
                try
                {
                    // Resume the selected transaction
                    _currentTransaction = _transactionService.ResumeTransaction(dialog.SelectedTransaction.Id);
                    CartGrid.ItemsSource = _currentTransaction.Cart;
                    
                    UpdateTotal();
                    UpdateEmptyMessage();
                    UpdateHeldTransactionCount();

                    MessageBox.Show(
                        $"Transaction resumed!\n\n" +
                        $"Items: {_currentTransaction.Cart.Count}\n" +
                        $"Total: {_currentTransaction.Total:C}\n" +
                        $"Age Verified: {(_currentTransaction.AgeVerified ? "Yes" : "No")}",
                        "Transaction Resumed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error resuming transaction: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double total = _currentTransaction.Total;
            double usd = total / 1.38; // CAD to USD conversion

            MessageBox.Show(
                $"CURRENCY CONVERSION\n\n" +
                $"CAD: ${total:F2}\n" +
                $"USD: ${usd:F2}\n\n" +
                $"Exchange Rate: 1 USD = 1.38 CAD",
                "USD ↔ CAD Conversion",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTransaction.Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add items before proceeding to payment.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                "Card/Other Payment\n\n" +
                "This feature allows payment via:\n" +
                "• Credit Card\n" +
                "• Debit Card\n" +
                "• Mobile Payment\n\n" +
                "(Feature to be fully implemented)",
                "Payment Options",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTransaction.Cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add items before proceeding to payment.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var payment = new PaymentWindow(_currentTransaction.Total, () =>
            {
                // Update product stock after successful payment
                foreach (var item in _currentTransaction.Cart)
                {
                    item.Product.UpdateStock(-item.Quantity);
                }

                // Complete the transaction
                _transactionService.CompleteTransaction(_currentTransaction);
                TransactionHistory.Add(_currentTransaction);

                // ask user for receipt
                var result = MessageBox.Show("Print receipt?", "Receipt",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var receipt = new ReceiptWindow(_currentTransaction.Cart);
                    receipt.ShowDialog();
                }

                // Create new transaction
                _currentTransaction = _transactionService.CreateTransaction();
                CartGrid.ItemsSource = _currentTransaction.Cart;
                UpdateTotal();
                UpdateEmptyMessage();
            });

            payment.ShowDialog();
        }

        private void Receipt_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new TransactionHistoryWindow();
            historyWindow.ShowDialog();
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
