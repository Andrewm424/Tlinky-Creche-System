using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlinky.AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddChildFieldToIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9eQrcAgFctcjhDjYwD7iCezkSv3DuRwNM8CTIZLuKXoVMlcHdijBC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$pMQlyY/59CZZ43awOAQtIeCS0vhEXx27bVf2/IuKrFiDm4HD9m0E6");
        }
    }
}
