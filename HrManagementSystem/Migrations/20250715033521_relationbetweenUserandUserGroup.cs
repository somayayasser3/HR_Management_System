using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class relationbetweenUserandUserGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupID",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserGroups_GroupID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GroupID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GroupID",
                table: "AspNetUsers");
        }
    }
}
