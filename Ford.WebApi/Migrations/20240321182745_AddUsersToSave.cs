using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersToSave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_UserId",
                table: "Saves");

            migrationBuilder.DropIndex(
                name: "IX_Saves_UserId",
                table: "Saves");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Saves");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "AspNetUsers",
                newName: "LastUpdate");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Saves",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedByUserId",
                table: "Saves",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Saves_CreatedByUserId",
                table: "Saves",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Saves_LastUpdatedByUserId",
                table: "Saves",
                column: "LastUpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_CreatedByUserId",
                table: "Saves",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_LastUpdatedByUserId",
                table: "Saves",
                column: "LastUpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_CreatedByUserId",
                table: "Saves");

            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_LastUpdatedByUserId",
                table: "Saves");

            migrationBuilder.DropIndex(
                name: "IX_Saves_CreatedByUserId",
                table: "Saves");

            migrationBuilder.DropIndex(
                name: "IX_Saves_LastUpdatedByUserId",
                table: "Saves");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Saves");

            migrationBuilder.DropColumn(
                name: "LastUpdatedByUserId",
                table: "Saves");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "AspNetUsers",
                newName: "LastUpdatedDate");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Saves",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Saves_UserId",
                table: "Saves",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_UserId",
                table: "Saves",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
