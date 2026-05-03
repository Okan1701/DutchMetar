using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMetarCeiling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetarCeiling_Metars_MetarId",
                table: "MetarCeiling");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetarCeiling",
                table: "MetarCeiling");

            migrationBuilder.RenameTable(
                name: "MetarCeiling",
                newName: "MetarCeilings");

            migrationBuilder.RenameIndex(
                name: "IX_MetarCeiling_MetarId",
                table: "MetarCeilings",
                newName: "IX_MetarCeilings_MetarId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetarCeilings",
                table: "MetarCeilings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetarCeilings_Metars_MetarId",
                table: "MetarCeilings",
                column: "MetarId",
                principalTable: "Metars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetarCeilings_Metars_MetarId",
                table: "MetarCeilings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetarCeilings",
                table: "MetarCeilings");

            migrationBuilder.RenameTable(
                name: "MetarCeilings",
                newName: "MetarCeiling");

            migrationBuilder.RenameIndex(
                name: "IX_MetarCeilings_MetarId",
                table: "MetarCeiling",
                newName: "IX_MetarCeiling_MetarId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetarCeiling",
                table: "MetarCeiling",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetarCeiling_Metars_MetarId",
                table: "MetarCeiling",
                column: "MetarId",
                principalTable: "Metars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
