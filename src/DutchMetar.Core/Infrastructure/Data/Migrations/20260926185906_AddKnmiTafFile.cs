using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutchMetar.Core.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKnmiTafFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    FileContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnmiTafFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnmiTafFiles_FileName",
                table: "KnmiTafFiles",
                column: "FileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnmiTafFiles");
        }
    }
}
