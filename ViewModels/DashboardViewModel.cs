using MoodPlaylist.SQLite.Repository.Models;

namespace MoodPlaylistGenerator.ViewModels
{
    public class DashboardViewModel
    {
        public List<Song> RecentSongs { get; set; } = new();
        public List<Playlist> RecentPlaylists { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public Dictionary<int, int> MoodSongCounts { get; set; } = new();
        public int TotalSongs { get; set; }
        public int TotalPlaylists { get; set; }
    }
}
