using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace POSSystem
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<CartItem> Cart { get; set; }
		private string currentInput = "";
		private int pendingQuantity = 1;
		public MainWindow()
		{
			InitializeComponent();

			Cart = new ObservableCollection<CartItem>();

			DataContext = this;

			UpdateTotal();
			UpdateEmptyMessage();
		}

		private void Decrease_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is CartItem item)
			{
				int amount = pendingQuantity; //use qty input to decrease certain amount of items

				if (item.Quantity > amount)
				{
					item.Quantity -= amount;
				}
				else
				{
					Cart.Remove(item); //remove if qty = 1
				}

				pendingQuantity = 1;
				UpdateTotal();
				UpdateEmptyMessage();
				ResetInput();
			}
		}

		private void Increase_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is CartItem item)
			{
				int amount = pendingQuantity;

				item.Quantity += amount;

				pendingQuantity = 1;
				UpdateTotal();
				UpdateTotal();
			}
		}

		private void Number_Click(object sender, RoutedEventArgs e)
		{
			string value = (sender as Button).Content.ToString();

			if (value == "." && currentInput.Contains("."))
				return; // prevent multiple decimals

			if (value == "." && string.IsNullOrEmpty(currentInput))
			{
				currentInput = "0.";
			}

			if (string.IsNullOrEmpty(currentInput))
				currentInput = value;
			else
				currentInput += value;

			QuantityDisplay.Text = currentInput;
		}

		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			currentInput = "";
			QuantityDisplay.Text = "";
		}

		private void Back_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(currentInput))
			{
				currentInput = currentInput.Substring(0, currentInput.Length - 1);
			}

			QuantityDisplay.Text = currentInput;
		}

		private void Quantity_Click(object sender, RoutedEventArgs e)
		{
			pendingQuantity = int.TryParse(currentInput, out int result) ? result : 1;

			ResetInput(); // clears screen after pressing Qty
			QuantityDisplay.Text = $"⚠ Adjusting by:{pendingQuantity}";
		}

		private void Product_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;

			string name = btn.Content.ToString();
			double price = double.Parse(btn.Tag.ToString());

			int id = name.ToLower().GetHashCode();

			int qty = pendingQuantity;

			var existingItem = Cart.FirstOrDefault(x => x.Id == id);

			if (existingItem != null)
			{
				existingItem.Quantity += qty;
			}
			else
			{
				Cart.Add(new CartItem
				{
					Id = id,
					Name = name,
					Price = price,
					Quantity = qty
				});
			}

			pendingQuantity = 1; // reset pending quantity after adding to cart

			UpdateTotal();
			UpdateEmptyMessage();
			ResetInput();
		}

		private void ResetInput()
		{
			currentInput = "";
			QuantityDisplay.Text = "";
		}

		private void UpdateTotal()
		{
			double total = Cart.Sum(item => item.Subtotal);
			TotalText.Text = $"Total: {total:C}";
		}

		private void UpdateEmptyMessage()
		{
			if (Cart.Count == 0)
				EmptyMessage.Visibility = Visibility.Visible;
			else
				EmptyMessage.Visibility = Visibility.Collapsed;
		}

		private void Abort_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Cancel Transaction?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				Cart.Clear();
				UpdateTotal();
				UpdateEmptyMessage();
			}
		}

		private void Hold_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Transaction Held");
		}

		private void Convert_Click(object sender, RoutedEventArgs e)
		{
			double total = Cart.Sum(x => x.Subtotal);
			double cad = total * 1.38; // Example conversion rate

			MessageBox.Show($"USD: ${total:F2}\nCAD: ${cad:F2}");
		}

		private void Payment_Click(object sender, RoutedEventArgs e)
		{
			if (Cart.Count == 0)
			{
				MessageBox.Show("Cart is empty. Please add items before proceeding to payment.");
				return;
			}

			MessageBox.Show("Payment (Card/Other) ");
		}

		private void Cash_Click(object sender, RoutedEventArgs e)
		{
			if (Cart.Count == 0)
			{
				MessageBox.Show("Cart is empty. Please add items before proceeding to payment.");
				return;
			}

			double total = Cart.Sum(x => x.Subtotal);

			var payment = new PaymentWindow(total, () =>
			{
				Cart.Clear();
				UpdateTotal();
				UpdateEmptyMessage();
			});

			payment.ShowDialog();
		}

		private void Receipt_Click(object sender, RoutedEventArgs e)
		{
			var receipt = new ReceiptWindow(Cart);
			receipt.ShowDialog();
		}

		private void Category_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			MessageBox.Show($"Selected: {btn.Content}");
		}

		public class CartItem : INotifyPropertyChanged
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public double Price { get; set; }

			private int quantity;
			public int Quantity
			{
				get => quantity;
				set
				{
					quantity = value;
					OnPropertyChanged(nameof(Quantity));
					OnPropertyChanged(nameof(Subtotal));
				}
			}

			public double Subtotal => Quantity * Price;

			public event PropertyChangedEventHandler PropertyChanged;

			protected void OnPropertyChanged(string name)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}