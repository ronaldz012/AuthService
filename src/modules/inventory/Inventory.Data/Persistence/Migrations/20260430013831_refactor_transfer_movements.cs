using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class refactor_transfer_movements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockMovements_RelatedMovementId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_RelatedMovementId",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "RelatedMovementId",
                table: "StockMovements",
                newName: "stockTransferId");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 30, 1, 38, 30, 289, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 30, 1, 38, 30, 289, DateTimeKind.Utc).AddTicks(3867));

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_stockTransferId",
                table: "StockMovements",
                column: "stockTransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockTransfers_stockTransferId",
                table: "StockMovements",
                column: "stockTransferId",
                principalTable: "StockTransfers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockTransfers_stockTransferId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_stockTransferId",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "stockTransferId",
                table: "StockMovements",
                newName: "RelatedMovementId");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 29, 20, 5, 14, 465, DateTimeKind.Utc).AddTicks(3816));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 29, 20, 5, 14, 465, DateTimeKind.Utc).AddTicks(3719));

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_RelatedMovementId",
                table: "StockMovements",
                column: "RelatedMovementId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockMovements_RelatedMovementId",
                table: "StockMovements",
                column: "RelatedMovementId",
                principalTable: "StockMovements",
                principalColumn: "Id");
        }
    }
}
