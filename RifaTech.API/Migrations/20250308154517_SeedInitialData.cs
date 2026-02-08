using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RifaTech.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed data is now handled by DatabaseSeeder.cs at application startup
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to revert - seed data is managed by DatabaseSeeder
        }
    }
}
