using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlinky.AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Incidents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "ChildId",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Incidents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$pMQlyY/59CZZ43awOAQtIeCS0vhEXx27bVf2/IuKrFiDm4HD9m0E6");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ChildId",
                table: "Incidents",
                column: "ChildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_ChildId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ChildId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Incidents");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$OhocKAbMTrB.xZGkl1Kyqev5zJ2ZV0cG9wjH44W4mUfGro5l5FnHS");
        }
    }
}
