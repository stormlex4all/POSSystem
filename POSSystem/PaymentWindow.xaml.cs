using System.Windows;
using POSSystem.Models;

namespace POSSystem
{
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
            if (!double.TryParse(CashInput.Text, out double cash))
            {
                MessageBox.Show("Please enter a valid amount.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cash < totalAmount)
            {
                MessageBox.Show("Insufficient cash provided.", "Payment Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create payment record using OOP model
            var payment = new CashPayment(totalAmount, cash);
            var result = payment.ProcessPayment();

            MessageBox.Show($"Payment complete!\n{result}", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            onPaymentComplete?.Invoke();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
