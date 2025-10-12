using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<SongMood> SongMoods { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SongMood junction table
            modelBuilder.Entity<SongMood>()
                .HasKey(sm => new { sm.SongId, sm.MoodId });
            
            modelBuilder.Entity<SongMood>()
                .HasOne(sm => sm.Song)
                .WithMany(s => s.SongMoods)
                .HasForeignKey(sm => sm.SongId);
            
            modelBuilder.Entity<SongMood>()
                .HasOne(sm => sm.Mood)
                .WithMany(m => m.SongMoods)
                .HasForeignKey(sm => sm.MoodId);

            // Configure PlaylistSong junction table
            modelBuilder.Entity<PlaylistSong>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId });
            
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId);
            
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId);

            // Seed data for moods
            modelBuilder.Entity<Mood>().HasData(
                new Mood { Id = 1, Name = "Happy", Color = "#FFD700", Description = "Upbeat and energetic songs" },
                new Mood { Id = 2, Name = "Sad", Color = "#4169E1", Description = "Melancholic and emotional songs" },
                new Mood { Id = 3, Name = "Relaxed", Color = "#98FB98", Description = "Calm and soothing songs" },
                new Mood { Id = 4, Name = "Energetic", Color = "#FF6347", Description = "High-energy and motivating songs" },
                new Mood { Id = 5, Name = "Romantic", Color = "#FF69B4", Description = "Love songs and romantic ballads" },
                new Mood { Id = 6, Name = "Focus", Color = "#9370DB", Description = "Music for concentration and work" }
            );
        }
    }
}
