using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_feature_isMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMenu",
                table: "Features",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMenu",
                table: "Features");
        }
    }
}
