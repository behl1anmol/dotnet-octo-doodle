using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFcore.API.Migrations
{
    /// <inheritdoc />
    public partial class AddComoundKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies",
                columns: new[] { "Title", "ReleaseDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Movies_Title_ReleaseDate",
                table: "Movies");
        }
    }
}
