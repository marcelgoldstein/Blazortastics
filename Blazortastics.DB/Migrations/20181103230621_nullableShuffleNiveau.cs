using Microsoft.EntityFrameworkCore.Migrations;

namespace Blazortastics.DB.Migrations
{
    public partial class nullableShuffleNiveau : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ShuffleNiveau",
                table: "Rankings",
                nullable: true,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ShuffleNiveau",
                table: "Rankings",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
