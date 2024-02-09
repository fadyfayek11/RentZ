using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveFlagToCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "City",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7"), 0, "696c3428-3022-4778-976e-a92c0f618e07", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEC7L0j9A0X7J0+CgG7vuD2fVTvz7QcMR2cTgZzO24aF4hUlJTI83tsyaXsy5jhBFRw==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("5c121789-1e1d-427c-a55b-a6ecefcd31e7"));

            migrationBuilder.DropColumn(
                name: "IsActive",
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
    }
}
