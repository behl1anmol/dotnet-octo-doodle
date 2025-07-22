using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFcore.API.Migrations
{
    /// <inheritdoc />
    public partial class AlternateKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Genres_MainGenreId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_MainGenreId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MainGenreId",
                table: "Movies");

            migrationBuilder.AddColumn<string>(
                name: "MainGenreName",
                table: "Movies",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Genres_Name",
                table: "Genres",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_MainGenreName",
                table: "Movies",
                column: "MainGenreName");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Genres_MainGenreName",
                table: "Movies",
                column: "MainGenreName",
                principalTable: "Genres",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Genres_MainGenreName",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_MainGenreName",
                table: "Movies");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Genres_Name",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "MainGenreName",
                table: "Movies");

            migrationBuilder.AddColumn<int>(
                name: "MainGenreId",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_MainGenreId",
                table: "Movies",
                column: "MainGenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Genres_MainGenreId",
                table: "Movies",
                column: "MainGenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
