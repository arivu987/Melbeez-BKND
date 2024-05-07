using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Alter_Product_Location_And_MoveItemStatustransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "MovedItemStatusTransactons");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "MovedItemStatusTransactons",
                newName: "ItemId");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireOn",
                table: "MovedItemStatusTransactons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProduct",
                table: "MovedItemStatusTransactons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransferId",
                table: "MovedItemStatusTransactons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExpireOn",
                table: "MovedItemStatusTransactons");

            migrationBuilder.DropColumn(
                name: "IsProduct",
                table: "MovedItemStatusTransactons");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "MovedItemStatusTransactons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "MovedItemStatusTransactons",
                newName: "LocationId");

            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "MovedItemStatusTransactons",
                type: "bigint",
                nullable: true);
        }
    }
}
