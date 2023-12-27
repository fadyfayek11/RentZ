using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentZ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditPropertySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Properties",
                newName: "PriceTo");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Properties",
                newName: "PropertyType");

            migrationBuilder.AddColumn<double>(
                name: "PriceFrom",
                table: "Properties",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PropertyCategory",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Popular",
                table: "City",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ViewOrder",
                table: "City",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceFrom",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PropertyCategory",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Popular",
                table: "City");

            migrationBuilder.DropColumn(
                name: "ViewOrder",
                table: "City");

            migrationBuilder.RenameColumn(
                name: "PropertyType",
                table: "Properties",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "PriceTo",
                table: "Properties",
                newName: "Price");
        }
    }
}
