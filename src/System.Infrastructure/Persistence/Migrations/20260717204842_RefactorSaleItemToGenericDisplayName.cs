using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSaleItemToGenericDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductDisplayName",
                table: "SaleItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "SaleItems"
                SET "ProductDisplayName" = "ProductName" || ' (' || "ProductSku" || ') - ' || "ProductColorName" || ' / ' || "ProductSize"
                """);

            migrationBuilder.DropColumn(
                name: "ProductColorName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "ProductSize",
                table: "SaleItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductSize",
                table: "SaleItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "SaleItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductColorName",
                table: "SaleItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "SaleItems"
                SET "ProductName" = split_part("ProductDisplayName", ' (', 1),
                    "ProductColorName" = '',
                    "ProductSize" = ''
                """);

            migrationBuilder.DropColumn(
                name: "ProductDisplayName",
                table: "SaleItems");
        }
    }
}
