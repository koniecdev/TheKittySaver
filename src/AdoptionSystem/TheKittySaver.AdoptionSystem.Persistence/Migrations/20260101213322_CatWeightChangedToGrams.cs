using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheKittySaver.AdoptionSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatWeightChangedToGrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightValueInKilograms",
                table: "Cats");

            migrationBuilder.AddColumn<int>(
                name: "WeightValueInGrams",
                table: "Cats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightValueInGrams",
                table: "Cats");

            migrationBuilder.AddColumn<decimal>(
                name: "WeightValueInKilograms",
                table: "Cats",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
