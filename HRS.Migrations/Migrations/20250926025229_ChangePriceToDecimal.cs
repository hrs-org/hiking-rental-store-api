using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangePriceToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 2, 52, 29, 550, DateTimeKind.Utc).AddTicks(6110), "$2a$11$eN42syozahHg3eVte8K/I.kmNcYUNshzjs8GNUPefrvfINaHaAzVy", new DateTime(2025, 9, 26, 2, 52, 29, 550, DateTimeKind.Utc).AddTicks(6110) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Items",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 25, 10, 2, 25, 512, DateTimeKind.Utc).AddTicks(7750), "$2a$11$Gdm5YMKkKju/ftUFXxlZAOmHoK.UMNEBJQGgANtwOq5lFURKXdqeG", new DateTime(2025, 9, 25, 10, 2, 25, 512, DateTimeKind.Utc).AddTicks(7750) });
        }
    }
}
