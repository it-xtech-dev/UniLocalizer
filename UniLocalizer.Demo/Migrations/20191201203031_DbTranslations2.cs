using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class DbTranslations2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.DropIndex(
                name: "IX_BookTranslation_CultureName_BookId",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslationId",
                table: "BookTranslation");

            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "BookTranslation",
                type: "varchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                columns: new[] { "CultureName", "BookId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "BookTranslation",
                type: "varchar(5)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)");

            migrationBuilder.AddColumn<int>(
                name: "TranslationId",
                table: "BookTranslation",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                column: "TranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslation_CultureName_BookId",
                table: "BookTranslation",
                columns: new[] { "CultureName", "BookId" },
                unique: true,
                filter: "[CultureName] IS NOT NULL");
        }
    }
}
