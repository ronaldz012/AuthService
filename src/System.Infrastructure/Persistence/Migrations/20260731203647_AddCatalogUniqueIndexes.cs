using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Providers_TenantId_Name",
                table: "Providers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_TenantId_Sku",
                table: "ProductVariants",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colors_TenantId_Name",
                table: "Colors",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_TenantId_Name",
                table: "Brands",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_TenantId_Prefix",
                table: "Brands",
                columns: new[] { "TenantId", "Prefix" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_TenantId_Name",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_TenantId_Sku",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_Colors_TenantId_Name",
                table: "Colors");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Brands_TenantId_Name",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Brands_TenantId_Prefix",
                table: "Brands");
        }
    }
}
