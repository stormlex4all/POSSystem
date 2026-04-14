using System;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using POSSystem.Models;

namespace POSSystem
{
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow(ObservableCollection<CartItem> cart)
        {
            InitializeComponent();

            foreach (var item in cart)
            {
                ReceiptList.Items.Add($"{item.Product.Name} x{item.Quantity} - {item.Subtotal:C}");
            }

            double total = cart.Sum(x => x.Subtotal);
            TotalText.Text = $"Total: {total:C}";
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Printing receipt... (Feature to be implemented with actual printer)",
                "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
