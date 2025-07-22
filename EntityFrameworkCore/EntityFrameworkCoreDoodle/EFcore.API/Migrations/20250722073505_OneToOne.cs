using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFcore.API.Migrations
{
    /// <inheritdoc />
    public partial class OneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalInformation",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    ImdbUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RottenTomatoesUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TmdbUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalInformation", x => x.MovieId);
                    table.ForeignKey(
                        name: "FK_ExternalInformation_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalInformation");
        }
    }
}
