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
            // Step 1: Create the Stocks table first
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
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId",
                unique: true
            );

            // Step 2: Migrate data from Products to Stocks
            migrationBuilder.Sql(
                """
                INSERT INTO "Stocks" ("Id", "ProductId", "Quantity", "LowStockThreshold", "Version")
                SELECT gen_random_uuid(), "Id", "StockQuantity", "LowStockThreshold", '\x00000001'
                FROM "Products";
                """
            );

            // Step 3: Add StockId column to StockReservations (nullable first)
            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "StockReservations",
                type: "uuid",
                nullable: true
            );

            // Step 4: Populate StockId from Stocks table
            migrationBuilder.Sql(
                """
                UPDATE "StockReservations" sr
                SET "StockId" = s."Id"
                FROM "Stocks" s
                WHERE sr."ProductId" = s."ProductId";
                """
            );

            // Step 5: Make StockId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "StockId",
                table: "StockReservations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true
            );

            // Step 6: Add row version column
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StockReservations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            // Step 7: Create index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_StockId",
                table: "StockReservations",
                column: "StockId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                table: "StockReservations",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            // Step 8: Now safe to drop columns from Products
            migrationBuilder.DropColumn(name: "LowStockThreshold", table: "Products");

            migrationBuilder.DropColumn(name: "StockQuantity", table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Re-add columns to Products first
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

            // Step 2: Migrate data back from Stocks to Products
            migrationBuilder.Sql(
                """
                UPDATE "Products" p
                SET "StockQuantity" = s."Quantity", "LowStockThreshold" = s."LowStockThreshold"
                FROM "Stocks" s
                WHERE p."Id" = s."ProductId";
                """
            );

            // Step 3: Drop FK and index
            migrationBuilder.DropForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                table: "StockReservations"
            );

            migrationBuilder.DropIndex(
                name: "IX_StockReservations_StockId",
                table: "StockReservations"
            );

            // Step 4: Drop Stocks table
            migrationBuilder.DropTable(name: "Stocks");

            // Step 5: Drop StockId from StockReservations
            migrationBuilder.DropColumn(name: "StockId", table: "StockReservations");

            migrationBuilder.DropColumn(name: "xmin", table: "StockReservations");
        }
    }
}
