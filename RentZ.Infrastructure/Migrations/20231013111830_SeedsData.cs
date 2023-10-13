using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "45ebc48e-b867-4847-a1e6-ba1f275fc406", "1", "RootAdmin", "RootAdmin" },
                    { "9f4cbe69-c735-46d0-9634-4cf435c46184", "2", "Admin", "Admin" },
                    { "fb7f4a16-6f0b-4fa9-9f94-4db50b98014b", "3", "Client", "Client" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
