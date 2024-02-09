using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderReceiverIdsToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9"));

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Conversations",
                newName: "IsReadBySender");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverId",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SenderId",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsReadByReceiver",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Clients_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Clients_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Clients_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Clients_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");

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
                name: "ReceiverId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsReadByReceiver",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "IsReadBySender",
                table: "Conversations",
                newName: "IsRead");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9"), 0, "0b82e751-7f99-422e-b5b6-dc7db0ba2713", null, "admin@rentz.com", true, true, false, null, "ADMIN@RENTZ.COM", "ADMIN@RENTZ.COM", "AQAAAAIAAYagAAAAEB8IfRzgmzySgD59Sn4RLZrNNcQgdRZ89R0rNo8zFL+3L0VaIpdRHhtg4med06dGLQ==", null, false, null, false, "admin@rentz.com" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "IsRoot" },
                values: new object[] { new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9"), true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("45ebc48e-b867-4847-a1e6-ba1f275fc406"), new Guid("848ef153-27b6-4cee-a788-fc7b72bc24a9") });
        }
    }
}
