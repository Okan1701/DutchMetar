using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddsExtractedRawMetar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedRawMetar",
                table: "KnmiMetarFiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedRawMetar",
                table: "KnmiMetarFiles");
        }
    }
}
