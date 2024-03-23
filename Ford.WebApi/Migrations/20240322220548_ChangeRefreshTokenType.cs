using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRefreshTokenType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "varchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "varchar(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)");
        }
    }
}
