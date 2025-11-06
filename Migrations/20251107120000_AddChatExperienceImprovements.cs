using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddChatExperienceImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAt",
                table: "UserChats",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Bio",
                value: "Руководитель чатов");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Bio",
                value: "Следит за порядком");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Bio",
                value: "Всегда на связи");

            var stamp = new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc);

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 1 },
                column: "LastReadAt",
                value: stamp.AddTicks(4516));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 2 },
                column: "LastReadAt",
                value: stamp.AddTicks(4784));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 3 },
                column: "LastReadAt",
                value: stamp.AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 1 },
                column: "LastReadAt",
                value: stamp.AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 2 },
                column: "LastReadAt",
                value: stamp.AddTicks(4787));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 },
                column: "LastReadAt",
                value: stamp.AddTicks(4788));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 7, 1 },
                column: "LastReadAt",
                value: stamp.AddTicks(4789));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 7, 2 },
                column: "LastReadAt",
                value: stamp.AddTicks(4790));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 8, 1 },
                column: "LastReadAt",
                value: stamp.AddTicks(4790));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 8, 3 },
                column: "LastReadAt",
                value: stamp.AddTicks(4791));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 9, 2 },
                column: "LastReadAt",
                value: stamp.AddTicks(4792));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 9, 3 },
                column: "LastReadAt",
                value: stamp.AddTicks(4793));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "UserChats");
        }
    }
}
