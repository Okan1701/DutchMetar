using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Metars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    IsAuto = table.Column<bool>(type: "bit", nullable: false),
                    IsCavok = table.Column<bool>(type: "bit", nullable: false),
                    IsCorrected = table.Column<bool>(type: "bit", nullable: false),
                    WindDirection = table.Column<int>(type: "int", nullable: true),
                    WindSpeedKnots = table.Column<int>(type: "int", nullable: true),
                    WindSpeedGustsKnots = table.Column<int>(type: "int", nullable: true),
                    NoCloudsDetected = table.Column<bool>(type: "bit", nullable: false),
                    VisibilityMeters = table.Column<int>(type: "int", nullable: true),
                    RawMetar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TemperatureCelsius = table.Column<int>(type: "int", nullable: true),
                    DewpointCelsius = table.Column<int>(type: "int", nullable: true),
                    AltimeterValue = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metars_Airports_AirportId",
                        column: x => x.AirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetarCeiling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetarId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetarCeiling", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetarCeiling_Metars_MetarId",
                        column: x => x.MetarId,
                        principalTable: "Metars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetarCeiling_MetarId",
                table: "MetarCeiling",
                column: "MetarId");

            migrationBuilder.CreateIndex(
                name: "IX_Metars_AirportId",
                table: "Metars",
                column: "AirportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetarCeiling");

            migrationBuilder.DropTable(
                name: "Metars");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
