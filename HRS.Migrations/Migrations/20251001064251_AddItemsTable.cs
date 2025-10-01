using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 1, 6, 42, 50, 928, DateTimeKind.Utc).AddTicks(3000), "$2a$11$I4lDIgHSLPn4Ct2DX9M6WuMx2yomN50Y0X8pg4/.aAXfb555RpP4a", new DateTime(2025, 10, 1, 6, 42, 50, 928, DateTimeKind.Utc).AddTicks(3000) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 4, 29, 39, 202, DateTimeKind.Utc).AddTicks(3800), "$2a$11$ZOKhao/PqZpn1EiJcqnX2.fBU9oCsQbMtd0vaMGqgp8IgZE.Lw/ie", new DateTime(2025, 9, 26, 4, 29, 39, 202, DateTimeKind.Utc).AddTicks(3800) });
        }
    }
}
