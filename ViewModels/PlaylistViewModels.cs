using System.ComponentModel.DataAnnotations;
using MoodPlaylist.SQLite.Repository.Models;

namespace MoodPlaylistGenerator.ViewModels
{
    public class GeneratePlaylistViewModel
    {
        [Required]
        [Display(Name = "Select Mood")]
        public int SelectedMoodId { get; set; }

        [Range(1, 50, ErrorMessage = "Song count must be between 1 and 50")]
        [Display(Name = "Number of Songs")]
        public int SongCount { get; set; } = 10;

        [StringLength(100)]
        [Display(Name = "Playlist Name (Optional)")]
        public string? PlaylistName { get; set; }

        public List<Mood> AvailableMoods { get; set; } = new();
        public Dictionary<int, int> MoodSongCounts { get; set; } = new();
    }

    public class PlaylistDetailViewModel
    {
        public Playlist Playlist { get; set; } = null!;
        public List<PlaylistSong> Songs { get; set; } = new();
        public bool CanEdit { get; set; } = true;
    }

    public class PlaylistListViewModel
    {
        public List<Playlist> Playlists { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public int? FilterMoodId { get; set; }
    }

    public class EditPlaylistNameViewModel
    {
        public int PlaylistId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;
    }
}
