using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverMaestro.Migrations
{
    /// <inheritdoc />
    public partial class init9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Card",
                newName: "ProductId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Card",
                newName: "OrderId");
        }
    }
}
