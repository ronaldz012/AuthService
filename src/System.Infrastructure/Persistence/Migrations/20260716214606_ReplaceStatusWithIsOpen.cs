using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStatusWithIsOpen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CashRegisterClosures");

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "CashRegisterClosures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisterClosures_Tenant_Branch_OpenOnly",
                table: "CashRegisterClosures",
                columns: new[] { "TenantId", "BranchId" },
                unique: true,
                filter: "\"IsOpen\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CashRegisterClosures_Tenant_Branch_OpenOnly",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "CashRegisterClosures");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CashRegisterClosures",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
