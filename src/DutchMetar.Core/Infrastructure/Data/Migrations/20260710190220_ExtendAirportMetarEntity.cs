using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendAirportMetarEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrendType",
                table: "Metars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FileContent",
                table: "KnmiMetarFiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrendType",
                table: "Metars");

            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "KnmiMetarFiles");
        }
    }
}
