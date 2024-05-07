using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Added_IsVerifiedByAdmin_flag_in_AspNetUsers_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerifiedByAdmin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerifiedByAdmin",
                table: "AspNetUsers");
        }
    }
}
