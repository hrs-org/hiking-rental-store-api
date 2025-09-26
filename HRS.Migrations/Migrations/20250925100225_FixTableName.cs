using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Item_ParentId",
                table: "Item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Item",
                table: "Item");

            migrationBuilder.RenameTable(
                name: "Item",
                newName: "Items");

            migrationBuilder.RenameIndex(
                name: "IX_Item_ParentId",
                table: "Items",
                newName: "IX_Items_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 25, 10, 2, 25, 512, DateTimeKind.Utc).AddTicks(7750), "$2a$11$Gdm5YMKkKju/ftUFXxlZAOmHoK.UMNEBJQGgANtwOq5lFURKXdqeG", new DateTime(2025, 9, 25, 10, 2, 25, 512, DateTimeKind.Utc).AddTicks(7750) });

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_ParentId",
                table: "Items",
                column: "ParentId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_ParentId",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.RenameTable(
                name: "Items",
                newName: "Item");

            migrationBuilder.RenameIndex(
                name: "IX_Items_ParentId",
                table: "Item",
                newName: "IX_Item_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Item",
                table: "Item",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 25, 9, 5, 9, 661, DateTimeKind.Utc).AddTicks(1860), "$2a$11$bqqk7jXdVsyjm/waBMb1hOu9.HaA1jRAM76BW2efKVMVRLjTFfce6", new DateTime(2025, 9, 25, 9, 5, 9, 661, DateTimeKind.Utc).AddTicks(1860) });

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Item_ParentId",
                table: "Item",
                column: "ParentId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
