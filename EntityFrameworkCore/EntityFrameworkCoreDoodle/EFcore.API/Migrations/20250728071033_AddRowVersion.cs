using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFcore.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies");

            migrationBuilder.AddColumn<byte[]>(
                name: "ConcurrencyToken",
                table: "Genres",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Genres",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies",
                columns: new[] { "Title", "ReleaseDate" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_AgeRating",
                table: "Movies",
                column: "AgeRating",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_AgeRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Genres");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies",
                columns: new[] { "Title", "ReleaseDate" });
        }
    }
}
