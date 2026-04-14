using System.Windows;

namespace POSSystem
{
    /// <summary>
    /// Interaction logic for TransactionHistoryWindow.xaml
    /// </summary>
    public partial class TransactionHistoryWindow : Window
    {
        public TransactionHistoryWindow()
        {
            InitializeComponent();

            foreach (var t in MainWindow.TransactionHistory)
            {
                HistoryList.Items.Add($"{t.CreatedDate} - {t.Total:C}");
			}
		}

		private void View_Click(object sender, RoutedEventArgs e)
		{
            int index = HistoryList.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Please select a transaction to view");
                return;
            }
            var transaction = MainWindow.TransactionHistory[index];
            var receipt = new ReceiptWindow(transaction.Cart);

            receipt.ShowDialog();
		}
    }
}
