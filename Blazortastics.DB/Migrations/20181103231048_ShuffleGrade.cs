using Microsoft.EntityFrameworkCore.Migrations;

namespace Blazortastics.DB.Migrations
{
    public partial class ShuffleGrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShuffleNiveau",
                table: "Rankings",
                newName: "ShuffleGrade");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShuffleGrade",
                table: "Rankings",
                newName: "ShuffleNiveau");
        }
    }
}
