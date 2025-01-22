using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvccore_dotnet_app.Migrations
{
    /// <inheritdoc />
    public partial class AddRankToUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "UserRole");
        }
    }
}
