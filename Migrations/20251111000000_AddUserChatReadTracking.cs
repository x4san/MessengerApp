using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserChatReadTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAt",
                table: "UserChats",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastReadMessageId",
                table: "UserChats",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "UserChats");

            migrationBuilder.DropColumn(
                name: "LastReadMessageId",
                table: "UserChats");
        }
    }
}
