using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HorseUserAccessRoles",
                table: "HorseUsers",
                column: "AccessRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HorseUserAccessRoles",
                table: "HorseUsers");
        }
    }
}
