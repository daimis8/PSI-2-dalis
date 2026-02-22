using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSI.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class SwitchToTablePerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongCollection",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "Artist",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "SongCollection");

            migrationBuilder.DropColumn(
                name: "SongListType",
                table: "SongCollection");

            migrationBuilder.RenameTable(
                name: "SongCollection",
                newName: "Playlists");

            migrationBuilder.RenameIndex(
                name: "IX_SongCollection_CurrentSongId",
                table: "Playlists",
                newName: "IX_Playlists_CurrentSongId");

            migrationBuilder.AlterColumn<int>(
                name: "VisitCount",
                table: "Playlists",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Playlists",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Artist = table.Column<string>(type: "text", nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Popularity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Albums_AlbumId",
                table: "Songs",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Songs_CurrentSongId",
                table: "Playlists");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Playlists_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Albums_AlbumId",
                table: "Songs");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists");

            migrationBuilder.RenameTable(
                name: "Playlists",
                newName: "SongCollection");

            migrationBuilder.RenameIndex(
                name: "IX_Playlists_CurrentSongId",
                table: "SongCollection",
                newName: "IX_SongCollection_CurrentSongId");

            migrationBuilder.AlterColumn<int>(
                name: "VisitCount",
                table: "SongCollection",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "SongCollection",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "SongCollection",
                type: "text",
                nullable: true);

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongCollection",
                table: "SongCollection",
                column: "Id");

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
    }
}
