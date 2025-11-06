using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsGroup", "IsPrivate", "Name" },
                values: new object[,]
                {
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, true, "ЛС: Админ ↔ Модератор" },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, true, "ЛС: Админ ↔ Пользователь" },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, true, "ЛС: Модератор ↔ Пользователь" }
                });

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4516));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 2 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4784));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 2 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4787));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4788));

            migrationBuilder.InsertData(
                table: "UserChats",
                columns: new[] { "ChatId", "UserId", "IsAdmin", "JoinedAt" },
                values: new object[,]
                {
                    { 7, 1, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4789) },
                    { 8, 1, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4790) },
                    { 7, 2, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4790) },
                    { 9, 2, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4792) },
                    { 8, 3, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4791) },
                    { 9, 3, false, new DateTime(2025, 11, 6, 1, 6, 56, 789, DateTimeKind.Utc).AddTicks(4793) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 7, 1 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 9, 2 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 8, 3 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 9, 3 });

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(2744));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3006));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 2 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3003));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 2 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3006));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3005));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3007));
        }
    }
}
