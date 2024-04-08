using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLastModifiedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHorses");

            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                newName: "IX_NormalizedUserNames");

            migrationBuilder.RenameIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                newName: "IX_NormalizedUserEmails");

            migrationBuilder.AddColumn<long>(
                name: "LastModifiedByUserId",
                table: "Horses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HorseUsers",
                columns: table => new
                {
                    HorseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccessRole = table.Column<string>(type: "nvarchar(8)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseUsers", x => new { x.HorseId, x.UserId });
                    table.ForeignKey(
                        name: "FK_HorseUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HorseUsers_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "HorseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Horses_LastModifiedByUserId",
                table: "Horses",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseUsers_UserId",
                table: "HorseUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Horses_AspNetUsers_LastModifiedByUserId",
                table: "Horses",
                column: "LastModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Horses_AspNetUsers_LastModifiedByUserId",
                table: "Horses");

            migrationBuilder.DropTable(
                name: "HorseUsers");

            migrationBuilder.DropIndex(
                name: "IX_Horses_LastModifiedByUserId",
                table: "Horses");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Horses");

            migrationBuilder.RenameIndex(
                name: "IX_NormalizedUserNames",
                table: "AspNetUsers",
                newName: "UserNameIndex");

            migrationBuilder.RenameIndex(
                name: "IX_NormalizedUserEmails",
                table: "AspNetUsers",
                newName: "EmailIndex");

            migrationBuilder.CreateTable(
                name: "UserHorses",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HorseId = table.Column<long>(type: "bigint", nullable: false),
                    AccessRole = table.Column<string>(type: "nvarchar(8)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHorses", x => new { x.UserId, x.HorseId });
                    table.ForeignKey(
                        name: "FK_UserHorses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHorses_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "HorseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHorses_HorseId",
                table: "UserHorses",
                column: "HorseId");
        }
    }
}
