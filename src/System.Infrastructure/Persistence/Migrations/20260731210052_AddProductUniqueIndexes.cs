using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_CategoryId_BrandId_Name",
                table: "Products",
                columns: new[] { "TenantId", "CategoryId", "BrandId", "Name" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_InternalCode",
                table: "Products",
                columns: new[] { "TenantId", "InternalCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchInventories_TenantId_BranchId_ProductVariantId",
                table: "BranchInventories",
                columns: new[] { "TenantId", "BranchId", "ProductVariantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_CategoryId_BrandId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_InternalCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_BranchInventories_TenantId_BranchId_ProductVariantId",
                table: "BranchInventories");
        }
    }
}
