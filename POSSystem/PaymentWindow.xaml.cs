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
using System.Globalization;

namespace POSSystem
{
	/// <summary>
	/// Interaction logic for PaymentWindow.xaml
	/// </summary>
	public partial class PaymentWindow : Window
	{
		private double totalAmount = 0;
		private Action onPaymentComplete;
		public PaymentWindow(double total, Action onComplete)
		{
			InitializeComponent();
			totalAmount = total;
			onPaymentComplete = onComplete;
			TotalText.Text = $"Total: {totalAmount:C}";
		}

	private void Calculate_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(CashInput.Text, out double cash))
			{
				double change = cash - totalAmount;

				if (change < 0)
				{
					ChangeText.Text = "Not enough cash!";
				}
				else
				{
					ChangeText.Text = $"Change: {change:C}";
				}
			}
			else
			{
				ChangeText.Text = "Invalid input";
			}
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Payment complete!");
			onPaymentComplete?.Invoke();
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}