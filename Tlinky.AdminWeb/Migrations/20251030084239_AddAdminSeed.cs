using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlinky.AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🚫 Removed seeding updates to avoid modifying existing admin data
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed since no data is seeded
        }
    }
}
