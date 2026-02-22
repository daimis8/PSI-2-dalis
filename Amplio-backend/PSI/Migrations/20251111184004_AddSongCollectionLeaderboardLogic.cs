using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace PSI.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class AddSongCollectionLeaderboardLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Songs_CurrentSongId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Playlists_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists");

            migrationBuilder.RenameTable(
                name: "Playlists",
                newName: "SongCollection");

            migrationBuilder.RenameColumn(
                name: "PlaylistId",
                table: "SongCollection",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Playlists_CurrentSongId",
                table: "SongCollection",
                newName: "IX_SongCollection_CurrentSongId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Songs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "AlbumId",
                table: "Songs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "SongCollection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "SongCollection",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Popularity",
                table: "SongCollection",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "SongCollection",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SongListType",
                table: "SongCollection",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VisitCount",
                table: "SongCollection",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongCollection",
                table: "SongCollection",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumId",
                table: "Songs",
                column: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_SongCollection_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId",
                principalTable: "SongCollection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongCollection_Songs_CurrentSongId",
                table: "SongCollection",
                column: "CurrentSongId",
                principalTable: "Songs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_SongCollection_AlbumId",
                table: "Songs",
                column: "AlbumId",
                principalTable: "SongCollection",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_SongCollection_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_SongCollection_Songs_CurrentSongId",
                table: "SongCollection");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_SongCollection_AlbumId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_AlbumId",
                table: "Songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongCollection",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "Artist",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "SongListType",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "VisitCount",
                table: "SongCollection");

            migrationBuilder.RenameTable(
                name: "SongCollection",
                newName: "Playlists");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Playlists",
                newName: "PlaylistId");

            migrationBuilder.RenameIndex(
                name: "IX_SongCollection_CurrentSongId",
                table: "Playlists",
                newName: "IX_Playlists_CurrentSongId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Songs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Songs_CurrentSongId",
                table: "Playlists",
                column: "CurrentSongId",
                principalTable: "Songs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_Playlists_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
