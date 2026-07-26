using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnKit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleBlockTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    LanguageCode = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false),

                    ContentJson = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    ArticleBlockId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ArticleBlockTranslations",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ArticleBlockTranslations_ArticleBlocks_ArticleBlockId",
                        column: x => x.ArticleBlockId,
                        principalTable: "ArticleBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    LanguageCode = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false),

                    Title = table.Column<string>(
                        type: "nvarchar(300)",
                        maxLength: 300,
                        nullable: false),

                    Summary = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: false),

                    Status = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    ArticleId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ArticleTranslations",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ArticleTranslations_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBlockTranslations_ArticleBlockId_LanguageCode",
                table: "ArticleBlockTranslations",
                columns: new[]
                {
                    "ArticleBlockId",
                    "LanguageCode"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTranslations_ArticleId_LanguageCode",
                table: "ArticleTranslations",
                columns: new[]
                {
                    "ArticleId",
                    "LanguageCode"
                },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO [ArticleTranslations]
                    ([Id], [LanguageCode], [Title], [Summary], [Status], [ArticleId])
                SELECT
                    NEWID(),
                    N'en',
                    [Title],
                    COALESCE([Summary], N''),
                    [Status],
                    [Id]
                FROM [Articles];
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [ArticleBlockTranslations]
                    ([Id], [LanguageCode], [ContentJson], [ArticleBlockId])
                SELECT
                    NEWID(),
                    N'en',
                    [ContentJson],
                    [Id]
                FROM [ArticleBlocks];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleBlockTranslations");

            migrationBuilder.DropTable(
                name: "ArticleTranslations");
        }
    }
}
