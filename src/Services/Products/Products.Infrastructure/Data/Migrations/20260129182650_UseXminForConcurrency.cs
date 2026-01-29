using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseXminForConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old bytea Version column - xmin is a PostgreSQL system column that exists automatically
            migrationBuilder.DropColumn(name: "Version", table: "Stock");
            migrationBuilder.DropColumn(name: "Version", table: "Product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Stock",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Product",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]
            );
        }
    }
}
