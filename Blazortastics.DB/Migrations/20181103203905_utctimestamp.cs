using Microsoft.EntityFrameworkCore.Migrations;

namespace Blazortastics.DB.Migrations
{
    public partial class utctimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Rankings",
                newName: "UtcTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UtcTimestamp",
                table: "Rankings",
                newName: "Timestamp");
        }
    }
}
