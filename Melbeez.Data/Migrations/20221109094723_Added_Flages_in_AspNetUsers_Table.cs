using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melbeez.Data.Migrations
{
    public partial class Added_Flages_in_AspNetUsers_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBiometricAllowed",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThirdPartyServiceAllowed",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLoginAttempt",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationRemindedOn",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VerificationReminderCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBiometricAllowed",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "IsThirdPartyServiceAllowed",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "IsFirstLoginAttempt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationRemindedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationReminderCount",
                table: "AspNetUsers");
        }
    }
}
