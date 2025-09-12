using System.ComponentModel.DataAnnotations;

namespace MoodPlaylist.SQLite.Repository.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public int MoodId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Mood Mood { get; set; } = null!;
        public List<PlaylistSong> PlaylistSongs { get; set; } = new();
    }
}
