using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGovernorate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_Governorate_GovernorateId",
                table: "City");

            migrationBuilder.DropTable(
                name: "Governorate");

            migrationBuilder.DropIndex(
                name: "IX_City_GovernorateId",
                table: "City");

            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c"));

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "City");

            migrationBuilder.DropColumn(
                name: "Popular",
                table: "City");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc"), 0, "afb3a08c-3e36-468f-86cb-238f46c73a24", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEECcaWzhHnWVDN1w7d4Wd4FO+tPPyozc4N3q66awBrLtE24dWBXx2thj9KOFtxUvJg==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("6c3a7a80-0ea1-4f1e-a14d-986774987fcc"));

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "City",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Popular",
                table: "City",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Governorate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorate", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c"), 0, "78a77a46-fca4-4c2b-86c6-2257b8e7b261", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEEFVFJnRW7rY5JS3xk1HqTLzWiGLP6W/7vidugXLx1KT9kr2qHQT/id+ZCA886iElQ==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("cd990e28-e9f5-4ba6-a739-d1c31e2b5f0c") });

            migrationBuilder.CreateIndex(
                name: "IX_City_GovernorateId",
                table: "City",
                column: "GovernorateId");

            migrationBuilder.AddForeignKey(
                name: "FK_City_Governorate_GovernorateId",
                table: "City",
                column: "GovernorateId",
                principalTable: "Governorate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
