using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnKit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnKitContentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LearningStepId",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    LearningPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningZones_LearningPaths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalTable: "LearningPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    LearningZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningSteps_LearningZones_LearningZoneId",
                        column: x => x.LearningZoneId,
                        principalTable: "LearningZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_LearningStepId",
                table: "Articles",
                column: "LearningStepId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Key",
                table: "LearningPaths",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningSteps_LearningZoneId",
                table: "LearningSteps",
                column: "LearningZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningZones_Key",
                table: "LearningZones",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningZones_LearningPathId",
                table: "LearningZones",
                column: "LearningPathId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_LearningSteps_LearningStepId",
                table: "Articles",
                column: "LearningStepId",
                principalTable: "LearningSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_LearningSteps_LearningStepId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "LearningSteps");

            migrationBuilder.DropTable(
                name: "LearningZones");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.DropIndex(
                name: "IX_Articles_LearningStepId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "LearningStepId",
                table: "Articles");
        }
    }
}
