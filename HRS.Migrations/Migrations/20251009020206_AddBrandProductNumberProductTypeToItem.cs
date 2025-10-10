using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandProductNumberProductTypeToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Items",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductNumber",
                table: "Items",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "Items",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 2, 2, 5, 994, DateTimeKind.Utc).AddTicks(8029), "$2a$11$bKNV992AiCKni0hy6nkWk.dEEe/RLQXUpTtAJdaEaSmLlGTnGuHcq", new DateTime(2025, 10, 9, 2, 2, 5, 994, DateTimeKind.Utc).AddTicks(8029) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProductNumber",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Items");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 20, 59, 496, DateTimeKind.Utc).AddTicks(5870), "$2a$11$4Uo2uCQbVDlZ0mGygeCZhuQQvYiMUWyTHsu1SN.Nbq3/sGLwUrIUK", new DateTime(2025, 10, 7, 14, 20, 59, 496, DateTimeKind.Utc).AddTicks(5870) });
        }
    }
}
