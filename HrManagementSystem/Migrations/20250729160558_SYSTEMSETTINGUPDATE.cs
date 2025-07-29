using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SYSTEMSETTINGUPDATE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SettingName",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SettingType",
                table: "SystemSettings");

            migrationBuilder.RenameColumn(
                name: "SettingValue",
                table: "SystemSettings",
                newName: "Weekend1");

            migrationBuilder.RenameColumn(
                name: "SettingId",
                table: "SystemSettings",
                newName: "Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SystemSettings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "BonusValue",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeductionValue",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HoursRate",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Weekend2",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusValue",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DeductionValue",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "HoursRate",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Weekend2",
                table: "SystemSettings");

            migrationBuilder.RenameColumn(
                name: "Weekend1",
                table: "SystemSettings",
                newName: "SettingValue");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SystemSettings",
                newName: "SettingId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SystemSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SettingName",
                table: "SystemSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SettingType",
                table: "SystemSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);
        }
    }
}
