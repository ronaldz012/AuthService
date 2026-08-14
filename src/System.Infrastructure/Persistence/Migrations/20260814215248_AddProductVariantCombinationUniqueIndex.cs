using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantCombinationUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_TenantId_ProductId_ColorId_SizeId",
                table: "ProductVariants",
                columns: new[] { "TenantId", "ProductId", "ColorId", "SizeId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_TenantId_ProductId_ColorId_SizeId",
                table: "ProductVariants");
        }
    }
}
