using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fall2025_Project3_dllecorchick.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActorMovies_ActorId",
                table: "ActorMovies");

            migrationBuilder.CreateIndex(
                name: "IX_ActorMovies_ActorId_MovieId",
                table: "ActorMovies",
                columns: new[] { "ActorId", "MovieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActorMovies_ActorId_MovieId",
                table: "ActorMovies");

            migrationBuilder.CreateIndex(
                name: "IX_ActorMovies_ActorId",
                table: "ActorMovies",
                column: "ActorId");
        }
    }
}
