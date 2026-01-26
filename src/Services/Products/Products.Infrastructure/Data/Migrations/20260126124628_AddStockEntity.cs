using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LowStockThreshold", table: "Products");

            migrationBuilder.DropColumn(name: "StockQuantity", table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "StockReservations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StockReservations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<byte[]>(
                        type: "bytea",
                        rowVersion: true,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_StockId",
                table: "StockReservations",
                column: "StockId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                table: "StockReservations",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                table: "StockReservations"
            );

            migrationBuilder.DropTable(name: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_StockReservations_StockId",
                table: "StockReservations"
            );

            migrationBuilder.DropColumn(name: "StockId", table: "StockReservations");

            migrationBuilder.DropColumn(name: "xmin", table: "StockReservations");

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }
    }
}
