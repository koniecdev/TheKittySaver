using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheKittySaver.AdoptionSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexesConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persons_IdentityId",
                table: "Persons",
                column: "IdentityId",
                unique: true);
            
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IX_Persons_Email ON Persons(Email);
                CREATE UNIQUE INDEX IX_Persons_PhoneNumber ON Persons(PhoneNumber);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_IdentityId",
                table: "Persons");
            
            migrationBuilder.Sql(
                """
                DROP INDEX IX_Persons_Email ON Persons;
                DROP INDEX IX_Persons_PhoneNumber ON Persons;
                """);
        }
    }
}
