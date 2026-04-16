using POSSystem.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem
{
    public partial class ReceiptWindow : Window
    {
        private string paymentMethod;
        public ReceiptWindow(ObservableCollection<CartItem> cart, string method, double change)
        {
            InitializeComponent();

            paymentMethod = method;

            //Receipt Header Info
            ReceiptInfoText.Text = $"Receipt No: {Guid.NewGuid().ToString().Substring(0, 8)}\n" + $"{DateTime.Now:MM/dd/yyyy HH:mm:ss}";

            //Items List
			foreach (var item in cart)
            {
				string name = item.Product.Name;
				string qtyLine = $"{item.Quantity} x {item.Product.Price:C}";
				string totalLine = item.Subtotal.ToString("C");

				ReceiptList.Items.Add(name);
				ReceiptList.Items.Add($"{qtyLine.PadRight(20)}{totalLine}");
			}

			//Calculations
            int itemCount = cart.Sum(x => x.Quantity);
			double subtotal = cart.Sum(x => x.Subtotal);
            double tax = subtotal * 0.13;
            double total = subtotal + tax;

            //Summary
            ItemsCountText.Text = $"Items: {itemCount}";
            SubtotalText.Text = $"Subtotal: {subtotal:C}";
            TaxText.Text = $"Tax (13%): {tax:c}";
            TotalText.Text = $"Total: {total:C}";

			//Payment section
			PaymentText.Text = $"Payment: {paymentMethod}";
			CashText.Text = $"Cash: {total:C}";
			ChangeText.Text = $"Change: {change:C}";
		}

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // Create a PrintDialog
            PrintDialog printDialog = new();

            if (printDialog.ShowDialog() == true)
            {
                // Only prints receipt content
                printDialog.PrintVisual(ReceiptPanel, "Receipt");
            }
        }
    }
}
