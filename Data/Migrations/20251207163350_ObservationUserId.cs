using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kisse.Data.Migrations
{
    /// <summary>
    /// Forgotten UserId on Observations.
    /// </summary>
    public partial class ObservationUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_AspNetUsers_UserId",
                table: "Observations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_AspNetUsers_UserId",
                table: "Observations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_AspNetUsers_UserId",
                table: "Observations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Observations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_AspNetUsers_UserId",
                table: "Observations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
