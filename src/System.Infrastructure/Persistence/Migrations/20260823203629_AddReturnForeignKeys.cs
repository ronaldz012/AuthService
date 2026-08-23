using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalSaleId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalSaleItemId",
                table: "SaleItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OriginalSaleId",
                table: "Sales",
                column: "OriginalSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_OriginalSaleItemId",
                table: "SaleItems",
                column: "OriginalSaleItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_SaleItems_OriginalSaleItemId",
                table: "SaleItems",
                column: "OriginalSaleItemId",
                principalTable: "SaleItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Sales_OriginalSaleId",
                table: "Sales",
                column: "OriginalSaleId",
                principalTable: "Sales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_SaleItems_OriginalSaleItemId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Sales_OriginalSaleId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_OriginalSaleId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_OriginalSaleItemId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "OriginalSaleId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "OriginalSaleItemId",
                table: "SaleItems");
        }
    }
}
