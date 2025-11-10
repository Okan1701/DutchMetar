using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetarImportResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetarImportResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    AddedMetarCount = table.Column<int>(type: "int", nullable: false),
                    CorrectedMetarCount = table.Column<int>(type: "int", nullable: false),
                    AddedAirportCount = table.Column<int>(type: "int", nullable: false),
                    ExceptionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetarImportResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetarImportResults");
        }
    }
}
