using System;
using System.Collections.Generic;
using System.Text;

namespace POSSystem.Models
{
    public class Transaction
    {
    public DateTime Date { get; set; }
    public List<CartItem> Items { get; set; }
    public double Total { get; set; }
    }
}
