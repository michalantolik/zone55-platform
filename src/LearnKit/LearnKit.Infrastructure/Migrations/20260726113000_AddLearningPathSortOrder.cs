using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnKit.Infrastructure.Migrations
{
    public partial class AddLearningPathSortOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "LearningPaths",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "LearningPaths");
        }
    }
}
