using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class DbTranslations6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslatedObjectGuid",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslatedObjectGuid",
                table: "Books");

            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "BookTranslation",
                type: "varchar(5)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)");

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationEntryGuid",
                table: "BookTranslation",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                column: "TranslationEntryGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslationEntryGuid",
                table: "BookTranslation");

            migrationBuilder.AlterColumn<string>(
                name: "CultureName",
                table: "BookTranslation",
                type: "varchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TranslatedObjectGuid",
                table: "BookTranslation",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TranslatedObjectGuid",
                table: "Books",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                columns: new[] { "CultureName", "TranslatedObjectGuid" });
        }
    }
}
