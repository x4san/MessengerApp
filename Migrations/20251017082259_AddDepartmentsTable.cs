using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Терапия" },
                    { 2, "Хирургия" },
                    { 3, "Лаборатория" },
                    { 4, "Рентгенология" },
                    { 5, "Регистратура" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Department_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Department_DepartmentId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
