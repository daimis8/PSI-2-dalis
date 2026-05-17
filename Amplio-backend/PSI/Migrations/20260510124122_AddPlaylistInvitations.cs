using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSI.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaylistInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistInvitations_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistInvitations_Users_InviteeId",
                        column: x => x.InviteeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistInvitations_InviteeId",
                table: "PlaylistInvitations",
                column: "InviteeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistInvitations_PlaylistId_InviteeId",
                table: "PlaylistInvitations",
                columns: new[] { "PlaylistId", "InviteeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistInvitations");
        }
    }
}
