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
using System.Collections.ObjectModel;
using System.Linq;

namespace POSSystem
{
	/// <summary>
	/// Interaction logic for ReceiptWindow.xaml
	/// </summary>
	public partial class ReceiptWindow : Window
	{
		public ReceiptWindow(ObservableCollection<MainWindow.CartItem> cart)
		{
			InitializeComponent();

			foreach (var item in cart)
			{
				ReceiptList.Items.Add($"{item.Name} x{item.Quantity} - {item.Subtotal:C}");
			}

			double total = cart.Sum(x => x.Subtotal);
			TotalText.Text = $"Total: {total:C}";
		}

		private void Print_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Printing receipt...");
		}
	}
}
