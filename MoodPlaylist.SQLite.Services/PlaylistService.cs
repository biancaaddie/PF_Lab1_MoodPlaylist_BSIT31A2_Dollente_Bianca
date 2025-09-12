using Microsoft.EntityFrameworkCore;
using MoodPlaylist.SQLite.Repository;
using MoodPlaylist.SQLite.Repository.Models;

namespace MoodPlaylist.SQLite.Services
{
    public class PlaylistService
    {
        private readonly ApplicationDbContext _context;

        public PlaylistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Playlist> GeneratePlaylistAsync(int userId, int moodId, int songCount = 10, string? playlistName = null)
        {
            // Get songs for the specified mood
            var availableSongs = await _context.Songs
                .Include(s => s.SongMoods)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .ToListAsync();

            if (!availableSongs.Any())
                throw new InvalidOperationException("No songs found for the selected mood.");

            // Shuffle and take requested number of songs
            var random = new Random();
            var selectedSongs = availableSongs
                .OrderBy(x => random.Next())
                .Take(songCount)
                .ToList();

            // Get mood name for playlist
            var mood = await _context.Moods.FindAsync(moodId);
            var defaultName = $"{mood?.Name ?? "Mixed"} Playlist - {DateTime.Now:yyyy-MM-dd HH:mm}";

            // Create playlist
            var playlist = new Playlist
            {
                Name = playlistName ?? defaultName,
                UserId = userId,
                MoodId = moodId,
                GeneratedAt = DateTime.UtcNow
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Add songs to playlist
            for (int i = 0; i < selectedSongs.Count; i++)
            {
                _context.PlaylistSongs.Add(new PlaylistSong
                {
                    PlaylistId = playlist.Id,
                    SongId = selectedSongs[i].Id,
                    Position = i + 1
                });
            }

            await _context.SaveChangesAsync();
            
            return await GetPlaylistByIdAsync(playlist.Id, userId) ?? playlist;
        }

        public async Task<Playlist?> GetPlaylistByIdAsync(int playlistId, int userId)
        {
            return await _context.Playlists
                .Include(p => p.Mood)
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .ThenInclude(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);
        }

        public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId)
        {
            return await _context.Playlists
                .Include(p => p.Mood)
                .Include(p => p.PlaylistSongs)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.GeneratedAt)
                .ToListAsync();
        }

        public async Task<bool> DeletePlaylistAsync(int playlistId, int userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Playlist?> UpdatePlaylistNameAsync(int playlistId, int userId, string newName)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null)
                return null;

            playlist.Name = newName;
            await _context.SaveChangesAsync();

            return playlist;
        }

        public async Task<Dictionary<int, int>> GetMoodSongCountsAsync(int userId)
        {
            return await _context.SongMoods
                .Where(sm => sm.Song.UserId == userId)
                .GroupBy(sm => sm.MoodId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}
