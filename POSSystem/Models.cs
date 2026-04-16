using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POSSystem.Models
{
    // Base Entity class - demonstrates inheritance
    public abstract class BaseEntity : INotifyPropertyChanged
    {
        private string _id;
        private DateTime _createdDate;

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged();
            }
        }

        protected BaseEntity()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Interface for priceable items
    public interface IPriceable
    {
        double GetPrice();
        double GetTotalPrice();
    }

    // Interface for taxable items
    public interface ITaxable
    {
        double CalculateTax(double taxRate);
    }

    // Product class - inherits from BaseEntity and implements interfaces
    public class Product : BaseEntity, IPriceable
    {
        private string _name;
        private double _price;
        private int _stock;
        private string _category;
        private bool _requiresAgeVerification;
        private string _barcode;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public double Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Price cannot be negative");
                _price = value;
                OnPropertyChanged();
            }
        }

        public int Stock
        {
            get => _stock;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Stock cannot be negative");
                _stock = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresAgeVerification
        {
            get => _requiresAgeVerification;
            set
            {
                _requiresAgeVerification = value;
                OnPropertyChanged();
            }
        }

        public string Barcode
        {
            get => _barcode;
            set
            {
                _barcode = value;
                OnPropertyChanged();
            }
        }

        public Product()
        {
            Name = string.Empty;
            Category = "General";
            Barcode = string.Empty;
        }

        public Product(string name, double price, int stock, string category = "General", bool requiresAge = false)
        {
            Name = name;
            Price = price;
            Stock = stock;
            Category = category;
            RequiresAgeVerification = requiresAge;
            Barcode = string.Empty;
        }

        public double GetPrice()
        {
            return Price;
        }

        public double GetTotalPrice()
        {
            return Price;
        }

        public void UpdateStock(int quantity)
        {
            if (Stock + quantity < 0)
                throw new InvalidOperationException("Insufficient stock");
            Stock += quantity;
        }

        public override string ToString()
        {
            return $"{Name} - ${Price:F2}";
        }
    }

    // CartItem class - composition with Product
    public class CartItem : BaseEntity, IPriceable, ITaxable
    {
        private Product _product;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Quantity cannot be negative");
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Convenience properties for binding
        public string Name => Product?.Name ?? "Unknown";
        public double Price => Product?.Price ?? 0;
        public double Subtotal => Quantity * Price;

        public CartItem()
        {
            Quantity = 1;
        }

        public CartItem(Product product, int quantity = 1)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity;
        }

        public double GetPrice()
        {
            return Price;
        }

        public double GetTotalPrice()
        {
            return Subtotal;
        }

        public double CalculateTax(double taxRate)
        {
            return Subtotal * taxRate;
        }
    }

    // Abstract Payment class - demonstrates polymorphism
    public abstract class Payment
    {
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        protected Payment(double amount)
        {
            Amount = amount;
            PaymentDate = DateTime.Now;
        }

        public abstract string ProcessPayment();
    }

    // Concrete payment implementations
    public class CashPayment : Payment
    {
        public double AmountTendered { get; set; }
        public double Change => AmountTendered - Amount;

        public CashPayment(double amount, double tendered) : base(amount)
        {
            AmountTendered = tendered;
        }

        public override string ProcessPayment()
        {
            return $"Cash payment processed. Change: ${Change:F2}";
        }
    }

    public class CardPayment : Payment
    {
        public string CardType { get; set; }
        public string Last4Digits { get; set; }

        public CardPayment(double amount, string cardType, string last4) : base(amount)
        {
            CardType = cardType;
            Last4Digits = last4;
        }

        public override string ProcessPayment()
        {
            return $"{CardType} payment processed. Card ending in {Last4Digits}";
        }
    }

    public class USDPayment : Payment
    {
        public double ExchangeRate { get; set; }
        public double USDAmount => Amount / ExchangeRate;

        public USDPayment(double cadAmount, double exchangeRate) : base(cadAmount)
        {
            ExchangeRate = exchangeRate;
        }

        public override string ProcessPayment()
        {
            return $"USD payment processed. ${USDAmount:F2} USD = ${Amount:F2} CAD";
        }
    }
    // Transaction data model
    public class Transaction
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public ObservableCollection<CartItem> Cart { get; set; }
        public double Subtotal { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
        public string Status { get; set; } // "Active", "Held", "Completed", "Aborted"
        public bool AgeVerified { get; set; }
        public double Change { get; set; } // For completed transactions
        public string PaymentMethod { get; set; } = "";

		public Transaction()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            Cart = new ObservableCollection<CartItem>();
            Status = "Active";
            AgeVerified = false;
        }

        // Method to calculate totals based on cart items
        public void CalculateTotals()
        {
            Subtotal = Cart.Sum(item => item.Subtotal);
            Tax = Subtotal * 0.05; // 5% GST
            Total = Subtotal + Tax;
        }

        // Method to create a deep copy of the transaction (used for holding/resuming)
        public Transaction Clone()
        {
            var clone = new Transaction
            {
                Id = this.Id,
                CreatedDate = this.CreatedDate,
                Status = this.Status,
                AgeVerified = this.AgeVerified
            };

            foreach (var item in Cart)
            {
                clone.Cart.Add(new CartItem(item.Product, item.Quantity));
            }

            clone.CalculateTotals();
            return clone;
        }
    }
}
