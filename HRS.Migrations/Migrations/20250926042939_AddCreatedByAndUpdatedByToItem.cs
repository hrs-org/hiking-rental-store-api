using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByAndUpdatedByToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Items",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 4, 29, 39, 202, DateTimeKind.Utc).AddTicks(3800), "$2a$11$ZOKhao/PqZpn1EiJcqnX2.fBU9oCsQbMtd0vaMGqgp8IgZE.Lw/ie", new DateTime(2025, 9, 26, 4, 29, 39, 202, DateTimeKind.Utc).AddTicks(3800) });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedById",
                table: "Items",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UpdatedById",
                table: "Items",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_CreatedById",
                table: "Items",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_UpdatedById",
                table: "Items",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_CreatedById",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_UpdatedById",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_CreatedById",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_UpdatedById",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Items");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 2, 52, 29, 550, DateTimeKind.Utc).AddTicks(6110), "$2a$11$eN42syozahHg3eVte8K/I.kmNcYUNshzjs8GNUPefrvfINaHaAzVy", new DateTime(2025, 9, 26, 2, 52, 29, 550, DateTimeKind.Utc).AddTicks(6110) });
        }
    }
}
