using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Item_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 25, 9, 5, 9, 661, DateTimeKind.Utc).AddTicks(1860), "$2a$11$bqqk7jXdVsyjm/waBMb1hOu9.HaA1jRAM76BW2efKVMVRLjTFfce6", new DateTime(2025, 9, 25, 9, 5, 9, 661, DateTimeKind.Utc).AddTicks(1860) });

            migrationBuilder.CreateIndex(
                name: "IX_Item_ParentId",
                table: "Item",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 10, 18, 18, 40, 881, DateTimeKind.Utc).AddTicks(2270), "$2a$11$VzRmFuyyXsigazkYuA2RMuvFulVyMoIuHfsMkjW/1nAay38Qg0isS", new DateTime(2025, 9, 10, 18, 18, 40, 881, DateTimeKind.Utc).AddTicks(2270) });
        }
    }
}
