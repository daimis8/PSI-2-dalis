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
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<PlaylistInvitation> PlaylistInvitations { get; set; }


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

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<FriendRequest>()
                .Property(fr => fr.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId, fr.Status });

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.SentAt });

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => f.FriendId);

            modelBuilder.Entity<Block>()
                .HasOne(b => b.Blocker)
                .WithMany()
                .HasForeignKey(b => b.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Block>()
                .HasOne(b => b.Blocked)
                .WithMany()
                .HasForeignKey(b => b.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Block>()
                .HasIndex(b => new { b.BlockerId, b.BlockedId })
                .IsUnique();

            modelBuilder.Entity<PlaylistInvitation>()
                .HasOne(pi => pi.Playlist)
                .WithMany()
                .HasForeignKey(pi => pi.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistInvitation>()
                .HasOne(pi => pi.Invitee)
                .WithMany()
                .HasForeignKey(pi => pi.InviteeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlaylistInvitation>()
                .HasIndex(pi => new { pi.PlaylistId, pi.InviteeId })
                .IsUnique();

            modelBuilder.Entity<PlaylistInvitation>()
                .HasIndex(pi => pi.InviteeId);
        }
    }
}
