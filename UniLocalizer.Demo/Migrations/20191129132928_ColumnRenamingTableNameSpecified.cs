using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class ColumnRenamingTableNameSpecified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ResourceItems",
                table: "ResourceItems");

            migrationBuilder.RenameTable(
                name: "ResourceItems",
                newName: "LocalizerResourceItem");

            migrationBuilder.RenameColumn(
                name: "Culture",
                table: "LocalizerResourceItem",
                newName: "CultureName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalizerResourceItem",
                table: "LocalizerResourceItem",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalizerResourceItem",
                table: "LocalizerResourceItem");

            migrationBuilder.RenameTable(
                name: "LocalizerResourceItem",
                newName: "ResourceItems");

            migrationBuilder.RenameColumn(
                name: "CultureName",
                table: "ResourceItems",
                newName: "Culture");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResourceItems",
                table: "ResourceItems",
                column: "Id");
        }
    }
}
