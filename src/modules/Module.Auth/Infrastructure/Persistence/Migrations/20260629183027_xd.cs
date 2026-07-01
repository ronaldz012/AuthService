using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class xd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AllowedFeatureKeys",
                table: "Plans",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId",
                table: "Branches",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Tenants_TenantId",
                table: "Branches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Tenants_TenantId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_TenantId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "AllowedFeatureKeys",
                table: "Plans");
        }
    }
}
