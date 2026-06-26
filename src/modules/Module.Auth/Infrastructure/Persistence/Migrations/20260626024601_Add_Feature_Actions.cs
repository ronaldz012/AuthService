using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Feature_Actions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableActions",
                table: "Features",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableActions",
                table: "Features");
        }
    }
}
