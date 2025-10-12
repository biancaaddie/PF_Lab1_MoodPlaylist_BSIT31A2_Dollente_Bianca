using System.ComponentModel.DataAnnotations;
using MoodPlaylistGenerator.Data.Entities;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.ViewModels
{
    public class CreateSongViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        [Url]
        [Display(Name = "YouTube URL (optional)")]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Local media (audio/video)")]
        public IFormFile? MediaFile { get; set; }

        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class EditSongViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        [Url]
        [Display(Name = "YouTube URL (optional)")]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Replace local media (audio/video)")]
        public IFormFile? MediaFile { get; set; }

        [Display(Name = "Moods")]
        public List<int> SelectedMoodIds { get; set; } = new();

        public List<Mood> AvailableMoods { get; set; } = new();
    }

    public class SongListViewModel
    {
        public List<Song> Songs { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public int? SelectedMoodId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SongDetailViewModel
    {
        public Song Song { get; set; } = null!;
        public string YouTubeVideoId { get; set; } = string.Empty;
        public List<Mood> AssignedMoods { get; set; } = new();
        public string? LocalMediaUrl { get; set; }
        public bool IsVideo { get; set; }
        public string FallbackYouTubeUrl { get; set; } = string.Empty;
        public bool FileExists { get; set; }
    }
}
