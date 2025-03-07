using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RifaTech.API.Migrations
{
    /// <inheritdoc />
    public partial class campopricevalue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PriceValue",
                table: "Rifas",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceValue",
                table: "Rifas");
        }
    }
}