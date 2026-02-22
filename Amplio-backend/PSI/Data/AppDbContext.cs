using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PSI.Models;

namespace PSI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<User> Users { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary key for join table
            modelBuilder.Entity<PlaylistSong>()
                .HasKey(playlistSong => new { playlistSong.PlaylistId, playlistSong.SongId });

            // Configure relationship: PlaylistSong → Playlist
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(playlistSong => playlistSong.Playlist)
                .WithMany(playlist => playlist.Songs)
                .HasForeignKey(playlistSong => playlistSong.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship: PlaylistSong → Song
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(playlistSong => playlistSong.Song)
                .WithMany()
                .HasForeignKey(playlistSong => playlistSong.SongId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistSong>()
                .Property(ps => ps.AddedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure string property lengths and requirements
            modelBuilder.Entity<Song>()
                .Property(s => s.Link)
                .IsRequired()
                .HasConversion(
                    link => link.ToString(),
                    value => new SongLink(value)
                )
                .HasMaxLength(255);



            modelBuilder.Entity<Song>()
                .Property(song => song.Artist)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Song>()
                .Property(song => song.Link)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Song>()
                .Property(song => song.Genres)
                .HasConversion(
                    genres => JsonSerializer.Serialize(genres, (JsonSerializerOptions)null),
                    genresJson => JsonSerializer.Deserialize<List<Genre>>(genresJson, (JsonSerializerOptions)null) ?? new List<Genre>()
                );

        }
    }
}
