using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POSSystem.Migrations
{
    /// <inheritdoc />
    public partial class Create_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "General"),
                    RequiresAgeVerification = table.Column<bool>(type: "INTEGER", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Active"),
                    AgeVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    Change = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "Category", "CreatedDate", "Name", "Price", "RequiresAgeVerification", "Stock" },
                values: new object[,]
                {
                    { "3671e535-ce24-44e5-ad3f-e7bdd5ebc2fd", "", "Beverages", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3481), "Coke", 1.99m, false, 50 },
                    { "375b9ada-3514-4c44-afe9-32f2bc474dd7", "987654321", "18+", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3526), "Tobacco", 12.99m, true, 15 },
                    { "52790d49-f6cb-484d-b36d-24455de084b2", "", "Snacks", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3511), "Chocolate", 1.99m, false, 60 },
                    { "5b9ff24d-82b9-48a9-acbf-0ebebd8547e5", "", "Food", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3425), "Hot Dog", 3.49m, false, 25 },
                    { "92a791fd-90e6-414f-95bf-4faebfa26c64", "", "Beverages", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3493), "Water", 1.49m, false, 100 },
                    { "98129e0b-27d7-41e5-9d6c-1cb22abf40df", "", "Snacks", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3519), "Gum", 1.99m, false, 50 },
                    { "990ecef6-dfc6-48bb-8886-38df2eaac35c", "", "Food", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3475), "Pizza Slice", 4.99m, false, 15 },
                    { "9bc69bcb-8c19-44a2-a7da-633cb747554a", "", "Beverages", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3504), "Energy Drink", 3.99m, false, 30 },
                    { "b28ff8b5-4f9e-4a14-a454-e860d1b67ed8", "123456789", "18+", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3522), "Cigarettes", 15.99m, true, 20 },
                    { "b3600389-3d89-4276-9c80-14640078d9ee", "", "Misc", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3535), "Batteries", 6.99m, false, 25 },
                    { "cdba805f-9dbf-4956-81ee-5d6a740c2905", "", "Food", new DateTime(2026, 4, 16, 1, 20, 23, 649, DateTimeKind.Local).AddTicks(7601), "Sandwich", 5.99m, false, 20 },
                    { "cfc49cb7-3bc2-4e67-ac7a-4c64f3a1b529", "", "Snacks", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3508), "Chips", 2.49m, false, 40 },
                    { "f37c66b2-3b5c-46f2-9fda-a1ef9e597115", "", "Beverages", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3485), "Pepsi", 1.99m, false, 50 },
                    { "f6d729da-62c1-4efd-be96-751cd47abfd3", "", "Snacks", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3516), "Candy Bar", 1.49m, false, 75 },
                    { "fd5d4414-8ecf-4860-81ca-670d6c8642cb", "", "Misc", new DateTime(2026, 4, 16, 1, 20, 23, 652, DateTimeKind.Local).AddTicks(3529), "Magazine", 4.99m, false, 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_TransactionId",
                table: "CartItems",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
