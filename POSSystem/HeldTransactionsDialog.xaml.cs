using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using POSSystem.Services;

namespace POSSystem
{
    public partial class HeldTransactionsDialog : Window
    {
        private readonly ITransactionService _transactionService;
        public Transaction SelectedTransaction { get; private set; }

        public HeldTransactionsDialog(ITransactionService transactionService)
        {
            InitializeComponent();
            _transactionService = transactionService;
            LoadHeldTransactions();
        }

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
        }

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
