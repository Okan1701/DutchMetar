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
                    Icao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnmiMetarFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FileLastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsFileProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ExtractedRawMetar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnmiMetarFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnmiTafFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FileLastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsFileProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ExtractedRawTaf = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    FileContent = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnmiTafFiles", x => x.Id);
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
                    Source = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SourceFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TemperatureCelsius = table.Column<int>(type: "int", nullable: true),
                    DewpointCelsius = table.Column<int>(type: "int", nullable: true),
                    AltimeterValue = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TrendType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                name: "Tafs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirportId = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RawTaf = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SourceFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tafs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tafs_Airports_AirportId",
                        column: x => x.AirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetarCeilings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetarId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetarCeilings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetarCeilings_Metars_MetarId",
                        column: x => x.MetarId,
                        principalTable: "Metars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnmiMetarFiles_FileName",
                table: "KnmiMetarFiles",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnmiTafFiles_FileName",
                table: "KnmiTafFiles",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetarCeilings_MetarId",
                table: "MetarCeilings",
                column: "MetarId");

            migrationBuilder.CreateIndex(
                name: "IX_Metars_AirportId",
                table: "Metars",
                column: "AirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Tafs_AirportId",
                table: "Tafs",
                column: "AirportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnmiMetarFiles");

            migrationBuilder.DropTable(
                name: "KnmiTafFiles");

            migrationBuilder.DropTable(
                name: "MetarCeilings");

            migrationBuilder.DropTable(
                name: "Tafs");

            migrationBuilder.DropTable(
                name: "Metars");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
