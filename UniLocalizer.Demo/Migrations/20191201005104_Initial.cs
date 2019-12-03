using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OriginalTitle = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });



            migrationBuilder.CreateTable(
                name: "BookTranslation",
                columns: table => new
                {
                    TranslationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CultureName = table.Column<string>(type: "varchar(5)", nullable: true),
                    BookId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTranslation", x => x.TranslationId);
                    table.ForeignKey(
                        name: "FK_BookTranslation_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslation_BookId",
                table: "BookTranslation",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslation_CultureName_BookId",
                table: "BookTranslation",
                columns: new[] { "CultureName", "BookId" },
                unique: true,
                filter: "[CultureName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookTranslation");

            migrationBuilder.DropTable(
                name: "LocalizerResourceItem");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
