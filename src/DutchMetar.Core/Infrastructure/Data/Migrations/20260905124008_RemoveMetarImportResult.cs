using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMetarImportResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetarImportResults");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Tafs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Metars");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "MetarCeilings");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "KnmiMetarFiles");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Airports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "Tafs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "Metars",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "MetarCeilings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "KnmiMetarFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "Airports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetarImportResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddedAirportCount = table.Column<int>(type: "int", nullable: false),
                    AddedMetarCount = table.Column<int>(type: "int", nullable: false),
                    CorrectedMetarCount = table.Column<int>(type: "int", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedMetarParses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetarImportResults", x => x.Id);
                });
        }
    }
}
