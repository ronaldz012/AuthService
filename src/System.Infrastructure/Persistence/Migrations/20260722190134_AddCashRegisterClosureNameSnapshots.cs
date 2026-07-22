using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegisterClosureNameSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloseByName",
                table: "CashRegisterClosures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenByName",
                table: "CashRegisterClosures",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseByName",
                table: "CashRegisterClosures");

            migrationBuilder.DropColumn(
                name: "OpenByName",
                table: "CashRegisterClosures");
        }
    }
}
