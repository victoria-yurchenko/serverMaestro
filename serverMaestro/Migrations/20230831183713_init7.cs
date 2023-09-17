using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverMaestro.Migrations
{
    /// <inheritdoc />
    public partial class init7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "User",
                newName: "Password");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "User",
                newName: "PasswordHash");

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
