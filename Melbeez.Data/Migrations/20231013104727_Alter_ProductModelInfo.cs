using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Alter_ProductModelInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "ProductModelInformation",
                newName: "TypeofLicence");

            migrationBuilder.AddColumn<string>(
                name: "AutomotiveType",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Capacity",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryType",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlButtonPlacement",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CookingPower",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryDate",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufactureYear",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiseLevel",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberOfDoors",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherInfo",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneOS",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreenSize",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemType",
                table: "ProductModelInformation",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomotiveType",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "CategoryType",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "ControlButtonPlacement",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "CookingPower",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "ManufactureYear",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "NoiseLevel",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "NumberOfDoors",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "OtherInfo",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "PhoneOS",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "ScreenSize",
                table: "ProductModelInformation");

            migrationBuilder.DropColumn(
                name: "SystemType",
                table: "ProductModelInformation");

            migrationBuilder.RenameColumn(
                name: "TypeofLicence",
                table: "ProductModelInformation",
                newName: "SerialNumber");
        }
    }
}
