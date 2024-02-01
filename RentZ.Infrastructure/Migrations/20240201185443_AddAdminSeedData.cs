using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d"), 0, "5acb92dc-eef9-4062-98bf-a334561e3814", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEHLSJV11SZokLXRsPFyE0ipu47SngfdxVc/1L/M95/W0LL1tJpszV7n7ZLE4hIcEJA==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("09bf649b-cb9d-4186-9d2f-b1b991cc6a7d"));
        }
    }
}
