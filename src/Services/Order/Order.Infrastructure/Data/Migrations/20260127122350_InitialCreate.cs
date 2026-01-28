using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Order.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Order",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerEmail = table.Column<string>(
                    type: "character varying(320)",
                    maxLength: 320,
                    nullable: false
                ),
                Status = table.Column<int>(type: "integer", nullable: false),
                TotalAmount = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                RejectionReason = table.Column<string>(
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true
                ),
                CreatedAt = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                UpdatedAt = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: true
                ),
                Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Order", x => x.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "OrderItem",
            columns: table => new
            {
                OrderEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductName = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false
                ),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                UnitPrice = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItem", x => new { x.OrderEntityId, x.Id });
                table.ForeignKey(
                    name: "FK_OrderItem_Order_OrderEntityId",
                    column: x => x.OrderEntityId,
                    principalTable: "Order",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OrderItem");

        migrationBuilder.DropTable(name: "Order");
    }
}
