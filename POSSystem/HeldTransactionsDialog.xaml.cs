using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem
{
    public partial class HeldTransactionsDialog : Window
    {
        private readonly ITransactionService _transactionService;
        private Action _onHeldTransactionChange; //to trigger UI updates in MainWindow when held transactions change

        public Transaction SelectedTransaction { get; private set; }

        public HeldTransactionsDialog(ITransactionService transactionService, Action onHeldTransactionChange)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _onHeldTransactionChange = onHeldTransactionChange;
            LoadHeldTransactions();
        }

        // Load held transactions and update UI
        private void LoadHeldTransactions()
        {
            var heldTransactions = _transactionService.GetHeldTransactions();
            TransactionsDataGrid.ItemsSource = heldTransactions;

            if (heldTransactions.Count == 0)
            {
                EmptyMessage.Visibility = Visibility.Visible;
                TransactionsDataGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyMessage.Visibility = Visibility.Collapsed;
                TransactionsDataGrid.Visibility = Visibility.Visible;
            }

            _onHeldTransactionChange();
        }

        // Resume selected transaction
        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                SelectedTransaction = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a transaction to resume.", 
                    "No Selection", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }

        // Allow double-click to resume transaction
        private void TransactionsDataGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                SelectedTransaction = selected;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Remove selected held transaction
        private void RemoveTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                var result = MessageBox.Show("Are you sure you want to remove this held transaction?", 
                    "Confirm Removal", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _transactionService.GetHeldTransactions().Remove(selected);
                    LoadHeldTransactions();
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction to remove.", 
                    "No Selection", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
    }

    // Converter for bool to Yes/No display
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "✓ Yes" : "✗ No";
            }
            return "✗ No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
