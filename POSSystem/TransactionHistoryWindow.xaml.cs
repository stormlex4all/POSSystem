using POSSystem.Models;
using POSSystem.Services;
using System.Windows;

namespace POSSystem
{
    /// <summary>
    /// Interaction logic for TransactionHistoryWindow.xaml
    /// </summary>
    public partial class TransactionHistoryWindow : Window
    {
        private readonly ITransactionService _transactionService;

        public TransactionHistoryWindow(ITransactionService transactionService)
        {
            InitializeComponent();
            _transactionService = transactionService;
            LoadTransactionsHistory();

        }

        // Load transactions history and update UI
        private void LoadTransactionsHistory()
        {
            var transactionsHistory = _transactionService.GetTransactionsHistory();
            TransactionsDataGrid.ItemsSource = transactionsHistory;

            if (transactionsHistory.Count == 0)
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

        // Event handler for the View button click
        private void View_Click(object sender, RoutedEventArgs e)
		{
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
            {
                var receipt = new ReceiptWindow(selected.Cart, "Unknown", 0);

                receipt.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a transaction to view.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
		}

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
