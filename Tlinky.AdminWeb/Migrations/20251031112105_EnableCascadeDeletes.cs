using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlinky.AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_Classes_ClassId",
                table: "Children");

            migrationBuilder.DropForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nHARv6UL2K.oWLt4gandl.EkD.IqdJ2tq3G8MgEbIE2ZqxSAh2YaO");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Classes_ClassId",
                table: "Children",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "ParentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_Classes_ClassId",
                table: "Children");

            migrationBuilder.DropForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8I1ZBg5Mhu0YpcPpN3ShAuD.58N6djmfo6M8kIaKC7Lo0e68XIgIG");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Classes_ClassId",
                table: "Children",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "ParentId");
        }
    }
}
