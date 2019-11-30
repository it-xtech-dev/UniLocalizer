using Microsoft.EntityFrameworkCore.Migrations;

namespace UniLocalizer.Demo.Migrations
{
    public partial class ColumnRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResourceKey",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationKey",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "ResourceItems",
                type: "varchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ResourceItems",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceKey",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "LocationKey",
                table: "ResourceItems",
                type: "varchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "ResourceItems",
                type: "varchar(5)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)");
        }
    }
}
