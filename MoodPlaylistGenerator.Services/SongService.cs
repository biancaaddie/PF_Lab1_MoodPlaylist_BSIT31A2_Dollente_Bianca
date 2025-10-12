using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services
{
    public class SongService
    {
        private readonly ApplicationDbContext _context;

        public SongService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Song>> GetUserSongsAsync(int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song?> GetSongByIdAsync(int songId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);
        }

        public async Task<Song> CreateSongAsync(string title, string artist, string youtubeUrl, int userId, List<int> moodIds)
        {
            var song = new Song
            {
                Title = title,
                Artist = artist,
                YouTubeUrl = youtubeUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Add mood associations
            if (moodIds.Any())
            {
                foreach (var moodId in moodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetSongByIdAsync(song.Id, userId) ?? song;
        }

        public async Task<Song?> UpdateSongAsync(int songId, int userId, string title, string artist, string youtubeUrl, List<int> moodIds)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return null;

            // Update song properties
            song.Title = title;
            song.Artist = artist;
            song.YouTubeUrl = youtubeUrl;

            // Remove existing mood associations
            _context.SongMoods.RemoveRange(song.SongMoods);

            // Add new mood associations
            foreach (var moodId in moodIds)
            {
                song.SongMoods.Add(new SongMood
                {
                    SongId = song.Id,
                    MoodId = moodId
                });
            }

            await _context.SaveChangesAsync();
            return await GetSongByIdAsync(songId, userId);
        }

        public async Task<bool> DeleteSongAsync(int songId, int userId)
        {
            var song = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await _context.Moods.OrderBy(m => m.Name).ToListAsync();
        }

        public async Task<List<Song>> GetSongsByMoodAsync(int moodId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public string ExtractYouTubeVideoId(string url)
        {
            try
            {
                // Extract video ID from various YouTube URL formats
                var uri = new Uri(url);
                
                if (uri.Host.Contains("youtu.be"))
                {
                    return uri.AbsolutePath.TrimStart('/');
                }
                
                if (uri.Host.Contains("youtube.com"))
                {
                    var query = uri.Query;
                    if (query.Contains("v="))
                    {
                        var vIndex = query.IndexOf("v=") + 2;
                        var endIndex = query.IndexOf("&", vIndex);
                        if (endIndex == -1) endIndex = query.Length;
                        return query.Substring(vIndex, endIndex - vIndex);
                    }
                }
            }
            catch
            {
                // If URL parsing fails, return empty string
            }

            return "";
        }
        
        public async Task<Song> CreateSongWithMediaAsync(string title, string artist, int userId, List<int> moodIds, IFormFile? mediaFile = null, string? youtubeUrl = null)
        {
            var song = new Song
            {
                Title = title,
                Artist = artist,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            // Handle media file upload
            if (mediaFile != null && mediaFile.Length > 0)
            {
                // This will be handled by the controller since we need the MediaUploadService
                // The controller will set these properties after calling this method
                song.MediaType = MediaType.YouTube; // Will be updated by controller
            }
            else if (!string.IsNullOrEmpty(youtubeUrl))
            {
                song.YouTubeUrl = youtubeUrl;
                song.MediaType = MediaType.YouTube;
            }
            
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
            
            // Add mood associations
            if (moodIds.Any())
            {
                foreach (var moodId in moodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }
            
            return await GetSongByIdAsync(song.Id, userId) ?? song;
        }
        
        public string GetPlayableUrl(Song song, IMediaUploadService mediaUploadService)
        {
            // Check if it's a local media file
            if (song.MediaType != MediaType.YouTube && !string.IsNullOrEmpty(song.LocalFilePath))
            {
                // Check if file exists
                if (mediaUploadService.FileExists(song.LocalFilePath))
                {
                    return mediaUploadService.GetMediaUrl(song.LocalFilePath);
                }
            }
            
            // Fallback to YouTube URL or Rick Roll
            if (!string.IsNullOrEmpty(song.YouTubeUrl))
            {
                return song.YouTubeUrl;
            }
            
            return mediaUploadService.GetFallbackYouTubeUrl();
        }
    }
}
