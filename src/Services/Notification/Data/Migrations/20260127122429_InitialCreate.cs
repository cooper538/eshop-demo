using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.NotificationService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerType = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    ProcessedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ProcessedMessages",
                        x => new { x.MessageId, x.ConsumerType }
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_ProcessedAt",
                table: "ProcessedMessages",
                column: "ProcessedAt"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProcessedMessages");
        }
    }
}
