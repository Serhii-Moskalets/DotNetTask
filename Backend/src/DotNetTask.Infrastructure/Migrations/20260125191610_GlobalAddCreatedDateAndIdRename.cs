using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetTask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalAddCreatedDateAndIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tasks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "task_lists",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tags",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "comments",
                newName: "id");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_date",
                table: "user_task_access",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_date",
                table: "tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_date",
                table: "user_task_access");

            migrationBuilder.DropColumn(
                name: "created_date",
                table: "tags");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tasks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "task_lists",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tags",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "comments",
                newName: "Id");
        }
    }
}
