using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class DbTranslations7 : Migration
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

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookTranslation",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation",
                columns: new[] { "CultureName", "BookId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTranslation",
                table: "BookTranslation");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookTranslation",
                nullable: true,
                oldClrType: typeof(int));

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

            migrationBuilder.AddForeignKey(
                name: "FK_BookTranslation_Books_BookId",
                table: "BookTranslation",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
