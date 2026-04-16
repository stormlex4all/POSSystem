using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using POSSystem.Models;

using System.IO;

namespace POSSystem.Data
{
    public class POSDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "pos.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            optionsBuilder.ConfigureWarnings(optionsBuilder => optionsBuilder.Ignore(RelationalEventId.PendingModelChangesWarning));
            //optionsBuilder.UseSqlServer("Server=localhost;Database=POSSystem;Trusted_Connection=True;"); //if using sqlServer in future
        }

        public static void Initialize()
        {
            using var context = new POSDbContext();
            context.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(p => p.Barcode).HasMaxLength(100);
                entity.Property(p => p.CreatedDate).IsRequired();
                entity.HasData(InitializeSampleProducts());
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Quantity).IsRequired();
                entity.Property(c => c.CreatedDate).IsRequired();

                // CartItem -> Product (many-to-one)
                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey("ProductId")
                      .OnDelete(DeleteBehavior.Restrict);

                // Ignore computed properties
                entity.Ignore(c => c.Name);
                entity.Ignore(c => c.Price);
                entity.Ignore(c => c.Subtotal);
            });

            // Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Status).HasMaxLength(50).HasDefaultValue("Active");
                entity.Property(t => t.Subtotal).HasPrecision(18, 2);
                entity.Property(t => t.Tax).HasPrecision(18, 2);
                entity.Property(t => t.Total).HasPrecision(18, 2);
                entity.Property(t => t.Change).HasPrecision(18, 2);
                entity.Property(t => t.PaymentMethod).HasMaxLength(100);
                entity.Property(t => t.CreatedDate).IsRequired();

                // Transaction -> CartItems (one-to-many)
                entity.HasMany(t => t.Cart)
                      .WithOne()
                      .HasForeignKey("TransactionId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private List<Product> InitializeSampleProducts()
        {
            return new List<Product>
            {
                // Food items
                new Product("Sandwich", 5.99m, 20, "Food"),
                new Product("Hot Dog", 3.49m, 25, "Food"),
                new Product("Pizza Slice", 4.99m, 15, "Food"),

                // Beverages
                new Product("Coke", 1.99m, 50, "Beverages"),
                new Product("Pepsi", 1.99m, 50, "Beverages"),
                new Product("Water", 1.49m, 100, "Beverages"),
                new Product("Energy Drink", 3.99m, 30, "Beverages"),

                // Snacks
                new Product("Chips", 2.49m, 40, "Snacks"),
                new Product("Chocolate", 1.99m, 60, "Snacks"),
                new Product("Candy Bar", 1.49m, 75, "Snacks"),
                new Product("Gum", 1.99m, 50, "Snacks"),

                // Age-restricted items
                new Product("Cigarettes", 15.99m, 20, "18+", true) { Barcode = "123456789" },
                new Product("Tobacco", 12.99m, 15, "18+", true) { Barcode = "987654321" },

                // Miscellaneous
                new Product("Magazine", 4.99m, 30, "Misc"),
                new Product("Batteries", 6.99m, 25, "Misc"),
            };
        }
    }
}