using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Added_Email_in_SMLErrorLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SMLErrorLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "SMLErrorLogs");
        }
    }
}
