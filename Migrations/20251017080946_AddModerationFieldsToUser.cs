using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModeratorComment",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModeratorComment",
                table: "Users");
        }
    }
}
