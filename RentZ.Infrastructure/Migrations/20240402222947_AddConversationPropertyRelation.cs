using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationPropertyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9"));

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("682c7bb9-214d-48d1-9288-6c1275f11405"), 0, "86a28e76-f9a6-430a-8a47-0b2205d19d4e", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEC0hWlXfQzwwCaWjrPR/TVhGt6c1gVlv0RevsBlxVUzoZAps+2b0i62X5O7SBrongw==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("682c7bb9-214d-48d1-9288-6c1275f11405"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("682c7bb9-214d-48d1-9288-6c1275f11405") });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PropId",
                table: "Conversations",
                column: "PropId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Properties_PropId",
                table: "Conversations",
                column: "PropId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Properties_PropId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_PropId",
                table: "Conversations");

            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("682c7bb9-214d-48d1-9288-6c1275f11405"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("682c7bb9-214d-48d1-9288-6c1275f11405") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("682c7bb9-214d-48d1-9288-6c1275f11405"));

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9"), 0, "08699fe7-6779-48c7-aa74-c39f4b80ab5f", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEI+YL7QUXjp1aa560X2yg5limJIAyOd2IxGj40BXheSK9O8V5xe+D15uRE/TNWxURA==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("66fa9b24-3215-4070-bb5e-09c0973ec8c9") });
        }
    }
}
