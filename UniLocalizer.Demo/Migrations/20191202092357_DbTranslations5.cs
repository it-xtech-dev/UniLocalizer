using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class DbTranslations5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation");

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

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookTranslation",
                nullable: true,
                oldClrType: typeof(int));

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

            migrationBuilder.AddForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslatedObjectGuid",
                table: "BookTranslation");

            migrationBuilder.DropColumn(
                name: "TranslatedObjectGuid",
                table: "Books");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookTranslation",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

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
                defaultValueSql: "newsequentialid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                column: "TranslationGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
