using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRole_DeleteUserGroup_EditDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserGroups_GroupID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GroupID",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "GroupID",
                table: "AspNetUsers",
                newName: "Role");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "CheckOutTime",
                table: "Attendances",
                type: "time",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "CheckInTime",
                table: "Attendances",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceDate",
                table: "Attendances",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceDate",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "AspNetUsers",
                newName: "GroupID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckOutTime",
                table: "Attendances",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckInTime",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GroupID",
                table: "AspNetUsers",
                column: "GroupID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserGroups_GroupID",
                table: "AspNetUsers",
                column: "GroupID",
                principalTable: "UserGroups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
