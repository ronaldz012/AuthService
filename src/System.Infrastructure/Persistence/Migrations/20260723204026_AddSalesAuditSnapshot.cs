using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesAuditSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "StockReceptions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "StockReceptionItems");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "BranchInventories");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "StockTransfers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "StockTransfers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "StockReceptions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "StockReceptions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "StockReceptionItems",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "StockReceptionItems",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "StockMovements",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "StockMovements",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Providers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Providers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "ProductVariants",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "ProductVariants",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Products",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Products",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Colors",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Colors",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Categories",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Categories",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Brands",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Brands",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "BranchInventories",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "BranchInventories",
                newName: "DeletedBy");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StockTransfers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StockTransfers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StockTransfers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StockTransfers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StockTransferItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StockTransferItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StockTransferItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StockTransferItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "StockTransferItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StockTransferItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StockTransferItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "StockTransferItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StockTransferItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StockReceptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StockReceptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StockReceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StockReceptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StockReceptionItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StockReceptionItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StockReceptionItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StockReceptionItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StockMovements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StockMovements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StockMovements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Sales",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Sales",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Sales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Sales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SaleItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SaleItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "SaleItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SaleItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "SaleItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "SaleItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SaleItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "SaleItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "SaleItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Providers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Providers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Providers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ProductVariants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "ProductVariants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Colors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Colors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Colors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Colors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CashRegisterMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "CashRegisterMovements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CashRegisterMovements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CashRegisterMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "CashRegisterMovements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CashRegisterMovements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "CashRegisterMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "CashRegisterMovements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CashRegisterClosures",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CashRegisterClosures",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "CashRegisterClosures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CashRegisterClosures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CashRegisterClosures",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "CashRegisterClosures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CashRegisterClosures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "CashRegisterClosures",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "CashRegisterClosures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Brands",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Brands",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Brands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Brands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "BranchInventories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "BranchInventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "BranchInventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "BranchInventories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockReceptions");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StockReceptions");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StockReceptions");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StockReceptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockReceptionItems");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StockReceptionItems");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StockReceptionItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StockReceptionItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "CashRegisterMovements");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BranchInventories");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "BranchInventories");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "BranchInventories");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "BranchInventories");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StockTransfers",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StockTransfers",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StockReceptions",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StockReceptions",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StockReceptionItems",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StockReceptionItems",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StockMovements",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StockMovements",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Providers",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Providers",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ProductVariants",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ProductVariants",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Products",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Products",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Colors",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Colors",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Categories",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Categories",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Brands",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Brands",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "BranchInventories",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "BranchInventories",
                newName: "DeletedById");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "StockTransfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "StockReceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "StockReceptionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Providers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ProductVariants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Colors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Brands",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "BranchInventories",
                type: "uuid",
                nullable: true);
        }
    }
}
