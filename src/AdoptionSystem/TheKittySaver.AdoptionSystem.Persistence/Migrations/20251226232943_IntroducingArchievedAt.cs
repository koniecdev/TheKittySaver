using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheKittySaver.AdoptionSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntroducingArchievedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Persons",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Cats",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "AdoptionAnnouncements",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Cats");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "AdoptionAnnouncements");
        }
    }
}
