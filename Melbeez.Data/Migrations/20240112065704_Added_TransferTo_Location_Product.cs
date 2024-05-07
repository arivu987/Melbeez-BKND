using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Added_TransferTo_Location_Product : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransferTo",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferTo",
                table: "Locations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferTo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TransferTo",
                table: "Locations");
        }
    }
}
