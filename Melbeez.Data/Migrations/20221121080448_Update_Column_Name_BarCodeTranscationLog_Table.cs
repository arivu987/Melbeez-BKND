using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Update_Column_Name_BarCodeTranscationLog_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BarCodeId",
                table: "BarCodeTransactionLogs",
                newName: "BarCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BarCode",
                table: "BarCodeTransactionLogs",
                newName: "BarCodeId");
        }
    }
}
