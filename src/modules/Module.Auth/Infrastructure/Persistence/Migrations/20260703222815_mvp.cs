using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleFeaturePermissions_Features_FeatureKey",
                table: "RoleFeaturePermissions");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleFeaturePermissions_Features_FeatureKey",
                table: "RoleFeaturePermissions",
                column: "FeatureKey",
                principalTable: "Features",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleFeaturePermissions_Features_FeatureKey",
                table: "RoleFeaturePermissions");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleFeaturePermissions_Features_FeatureKey",
                table: "RoleFeaturePermissions",
                column: "FeatureKey",
                principalTable: "Features",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
