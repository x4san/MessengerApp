using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPredefinedChatsStatic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsGroup", "IsPrivate", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, false, "Общий чат" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, "ЛС: Администратор ↔ Модератор" },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, "ЛС: Администратор ↔ Пользователь" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, "ЛС: Модератор ↔ Пользователь" }
                });

            migrationBuilder.InsertData(
                table: "UserChats",
                columns: new[] { "ChatId", "UserId", "IsAdmin", "JoinedAt" },
                values: new object[,]
                {
                    { 1, 1, true, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(323) },
                    { 2, 1, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1098) },
                    { 3, 1, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1100) },
                    { 1, 2, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1096) },
                    { 2, 2, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1099) },
                    { 4, 2, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1101) },
                    { 1, 3, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1098) },
                    { 3, 3, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1100) },
                    { 4, 3, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1102) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
