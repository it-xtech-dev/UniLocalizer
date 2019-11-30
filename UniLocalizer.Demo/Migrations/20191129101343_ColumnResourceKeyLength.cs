using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class ColumnResourceKeyLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ResourceItems",
                type: "varchar(300)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(300)");
        }
    }
}
