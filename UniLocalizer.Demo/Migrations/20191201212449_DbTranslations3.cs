using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class DbTranslations3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationGuid",
                table: "BookTranslation",
                nullable: false,
                defaultValue: new Guid());

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                column: "TranslationGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslationGuid",
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
    }
}
