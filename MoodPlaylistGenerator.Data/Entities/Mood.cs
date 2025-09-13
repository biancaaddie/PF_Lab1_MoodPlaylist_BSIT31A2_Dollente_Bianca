using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Data.Entities
{
    public class Mood
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Color { get; set; } = "#3B82F6"; // Default blue
        public string Description { get; set; } = string.Empty;
        
        // Navigation properties
        public List<SongMood> SongMoods { get; set; } = new();
        public List<Playlist> Playlists { get; set; } = new();
    }
}
