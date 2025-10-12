namespace MoodPlaylistGenerator.Data.Entities
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
