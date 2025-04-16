using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DNET.Backend.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LoginProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "login_provider",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "login_provider",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
