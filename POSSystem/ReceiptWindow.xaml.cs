using POSSystem.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
            // Create a PrintDialog
            PrintDialog printDialog = new();

            if (printDialog.ShowDialog() == true)
            {
                // Print the entire window or a specific element
                printDialog.PrintVisual(this, "Receipt");
            }
        }
    }
}
