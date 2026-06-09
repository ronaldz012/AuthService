using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Data.Migrations
{
    /// <inheritdoc />
    public partial class referenceId_SM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockTransfers_stockTransferId",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "stockTransferId",
                table: "StockMovements",
                newName: "StockTransferId");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_stockTransferId",
                table: "StockMovements",
                newName: "IX_StockMovements_StockTransferId");

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockTransfers_StockTransferId",
                table: "StockMovements",
                column: "StockTransferId",
                principalTable: "StockTransfers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockTransfers_StockTransferId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "StockTransferId",
                table: "StockMovements",
                newName: "stockTransferId");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_StockTransferId",
                table: "StockMovements",
                newName: "IX_StockMovements_stockTransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockTransfers_stockTransferId",
                table: "StockMovements",
                column: "stockTransferId",
                principalTable: "StockTransfers",
                principalColumn: "Id");
        }
    }
}
