using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ford.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangePrimaryKeyInSaveBonesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveBones",
                table: "SaveBones");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveBones",
                table: "SaveBones",
                column: "SaveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveBones",
                table: "SaveBones");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveBones",
                table: "SaveBones",
                columns: new[] { "SaveId", "BoneId" });
        }
    }
}
