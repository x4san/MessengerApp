using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 1 });

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
                keyValues: new object[] { 3, 3 });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Общий чат всех сотрудников");

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { true, "Отдел: Терапия" });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { true, "Отдел: Хирургия" });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { true, "Отдел: Лаборатория" });

            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsGroup", "IsPrivate", "Name" },
                values: new object[,]
                {
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, false, "Отдел: Рентгенология" },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, false, "Отдел: Регистратура" }
                });

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
                keyValues: new object[] { 1, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3005));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3007));

            migrationBuilder.InsertData(
                table: "UserChats",
                columns: new[] { "ChatId", "UserId", "IsAdmin", "JoinedAt" },
                values: new object[] { 3, 2, false, new DateTime(2025, 11, 6, 0, 39, 35, 103, DateTimeKind.Utc).AddTicks(3006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "DepartmentId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "DepartmentId",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Общий чат");

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { false, "ЛС: Администратор ↔ Модератор" });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { false, "ЛС: Администратор ↔ Пользователь" });

            migrationBuilder.UpdateData(
                table: "Chats",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsGroup", "Name" },
                values: new object[] { false, "ЛС: Модератор ↔ Пользователь" });

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(323));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 2, 1 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1098));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 2 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1096));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 1, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1098));

            migrationBuilder.UpdateData(
                table: "UserChats",
                keyColumns: new[] { "ChatId", "UserId" },
                keyValues: new object[] { 4, 3 },
                column: "JoinedAt",
                value: new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1102));

            migrationBuilder.InsertData(
                table: "UserChats",
                columns: new[] { "ChatId", "UserId", "IsAdmin", "JoinedAt" },
                values: new object[,]
                {
                    { 3, 1, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1100) },
                    { 2, 2, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1099) },
                    { 4, 2, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1101) },
                    { 3, 3, false, new DateTime(2025, 11, 1, 14, 59, 13, 44, DateTimeKind.Utc).AddTicks(1100) }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "DepartmentId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "DepartmentId",
                value: 1);
        }
    }
}
