using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRAKTOSWEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTempPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TempPassword",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempPassword",
                table: "Users");
        }
    }
}
