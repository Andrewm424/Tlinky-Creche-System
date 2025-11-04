using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlinky.AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixChildCascadeAndDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Children_ChildId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$/54NUx00vCNguhOPBTEbVOsWB6Jcy1B17QOAoHqcq6r1vtVvPxBgi");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "ChildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Children_ChildId",
                table: "Payments",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "ChildId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Children_ChildId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nHARv6UL2K.oWLt4gandl.EkD.IqdJ2tq3G8MgEbIE2ZqxSAh2YaO");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Children_ChildId",
                table: "Incidents",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "ChildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Children_ChildId",
                table: "Payments",
                column: "ChildId",
                principalTable: "Children",
                principalColumn: "ChildId");
        }
    }
}
