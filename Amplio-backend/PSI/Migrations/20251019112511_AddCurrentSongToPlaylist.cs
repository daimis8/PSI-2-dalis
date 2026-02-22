using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace PSI.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class AddCurrentSongToPlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentSongId",
                table: "Playlists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_CurrentSongId",
                table: "Playlists",
                column: "CurrentSongId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Songs_CurrentSongId",
                table: "Playlists",
                column: "CurrentSongId",
                principalTable: "Songs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Songs_CurrentSongId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_CurrentSongId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "CurrentSongId",
                table: "Playlists");
        }
    }
}
