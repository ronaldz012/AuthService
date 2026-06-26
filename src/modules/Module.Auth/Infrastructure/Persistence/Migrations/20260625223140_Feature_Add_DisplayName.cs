using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Feature_Add_DisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Databases_DataBaseId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "Databases");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Features",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TenantDatabases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Schema = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDatabases", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantDatabases_DataBaseId",
                table: "Tenants",
                column: "DataBaseId",
                principalTable: "TenantDatabases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantDatabases_DataBaseId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "TenantDatabases");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Features");

            migrationBuilder.CreateTable(
                name: "Databases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Schema = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Databases", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Databases_DataBaseId",
                table: "Tenants",
                column: "DataBaseId",
                principalTable: "Databases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
