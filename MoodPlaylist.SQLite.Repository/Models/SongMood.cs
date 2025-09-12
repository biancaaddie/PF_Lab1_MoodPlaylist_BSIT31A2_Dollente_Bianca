namespace MoodPlaylist.SQLite.Repository.Models
{
    public class SongMood
    {
        public int SongId { get; set; }
        public int MoodId { get; set; }
        
        // Navigation properties
        public Song Song { get; set; } = null!;
        public Mood Mood { get; set; } = null!;
    }
}
