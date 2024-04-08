using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameSomeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_LastUpdatedByUserId",
                table: "Saves");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedByUserId",
                table: "Saves",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "Saves",
                newName: "LastModified");

            migrationBuilder.RenameIndex(
                name: "IX_Saves_LastUpdatedByUserId",
                table: "Saves",
                newName: "IX_Saves_LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "Horses",
                newName: "LastModified");

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_LastModifiedByUserId",
                table: "Saves",
                column: "LastModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_LastModifiedByUserId",
                table: "Saves");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Saves",
                newName: "LastUpdatedByUserId");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Saves",
                newName: "LastUpdate");

            migrationBuilder.RenameIndex(
                name: "IX_Saves_LastModifiedByUserId",
                table: "Saves",
                newName: "IX_Saves_LastUpdatedByUserId");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Horses",
                newName: "LastUpdate");

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_LastUpdatedByUserId",
                table: "Saves",
                column: "LastUpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
