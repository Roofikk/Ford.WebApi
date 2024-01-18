using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UserSaveMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_HorseOwner_HorseId",
                table: "HorseOwners",
                newName: "IX_HorseOwners_HorseId");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "Saves",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Saves",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                table: "Horses",
                type: "nvarchar(6)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "nvarchar(6)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Horses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorseOwners",
                table: "HorseOwners",
                columns: new[] { "UserId", "HorseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Saves_UserId",
                table: "Saves",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HorseOwners_AspNetUsers_UserId",
                table: "HorseOwners",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorseOwners_Horses_HorseId",
                table: "HorseOwners",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "HorseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saves_AspNetUsers_UserId",
                table: "Saves",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorseOwners_AspNetUsers_UserId",
                table: "HorseOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_HorseOwners_Horses_HorseId",
                table: "HorseOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_Saves_AspNetUsers_UserId",
                table: "Saves");

            migrationBuilder.DropIndex(
                name: "IX_Saves_UserId",
                table: "Saves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorseOwners",
                table: "HorseOwners");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Saves");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Horses");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "Saves",
                type: "nvarchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");

            migrationBuilder.AlterColumn<int>(
                name: "Sex",
                table: "Horses",
                type: "nvarchar(6)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldNullable: true);
        }
    }
}
