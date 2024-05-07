using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Added_TransferId_ItemTransferInvitation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransferId",
                table: "ItemTransferInvitations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "ItemTransferInvitations");
        }
    }
}
