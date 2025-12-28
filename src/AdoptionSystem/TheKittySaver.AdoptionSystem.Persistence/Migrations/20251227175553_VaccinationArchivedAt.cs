using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheKittySaver.AdoptionSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VaccinationArchivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Vaccinations",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Vaccinations");
        }
    }
}
