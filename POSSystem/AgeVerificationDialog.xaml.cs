using System.Windows;

namespace POSSystem
{
    public partial class AgeVerificationDialog : Window
    {
        public AgeVerificationDialog()
        {
            InitializeComponent();
            AgeTextBox.Focus();
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AgeTextBox.Text, out int age))
            {
                ValidationTextBlock.Text = "Please enter a valid age.";
                return;
            }

            if (age < 18)
            {
                ValidationTextBlock.Text = "Customer must be 18 years or older.";
                MessageBox.Show("Age verification failed. Customer is under 18.", 
                    "Verification Failed", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                DialogResult = false;
                Close();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
