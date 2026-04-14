using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using POSSystem.Models;
using System.Linq;
using System.Collections.ObjectModel;

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
                HistoryList.Items.Add($"{t.Date} - {t.Total:C}");
			}
		}

		private void View_Click(object sender, RoutedEventArgs e)
		{
            int index = HistoryList.SelectedIndex;
            if (index < 0) return;
            var transaction = MainWindow.TransactionHistory[index];
            var receipt = new ReceiptWindow(
            new ObservableCollection<CartItem>(transaction.Items));

            receipt.ShowDialog();
		}
    }
}
