using Microsoft.AspNetCore.Mvc;
using MoodPlaylistGenerator.Services.Interfaces;
using MoodPlaylistGenerator.Services;
using MoodPlaylistGenerator.Data;

namespace MoodPlaylistGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly SongService _songService;
        private readonly PlaylistService _playlistService;

        public TestController(
            ApplicationDbContext context,
            IAuthService authService,
            SongService songService,
            PlaylistService playlistService)
        {
            _context = context;
            _authService = authService;
            _songService = songService;
            _playlistService = playlistService;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { 
                message = "Backend is running!",
                database = "Connected",
                timestamp = DateTime.Now
            });
        }

        [HttpGet("moods")]
        public async Task<IActionResult> GetMoods()
        {
            try
            {
                var moods = await _songService.GetAllMoodsAsync();
                return Ok(moods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-user")]
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                var user = await _authService.RegisterAsync("test@test.com", "testuser", "password123");
                if (user == null)
                {
                    return BadRequest(new { error = "User already exists" });
                }
                return Ok(new { 
                    message = "Test user created successfully",
                    userId = user.Id,
                    email = user.Email,
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-song")]
        public async Task<IActionResult> CreateTestSong()
        {
            try
            {
                // First try to get or create a test user
                var user = await _authService.LoginAsync("testuser", "password123");
                if (user == null)
                {
                    user = await _authService.RegisterAsync("test2@test.com", "testuser2", "password123");
                    if (user == null)
                    {
                        return BadRequest(new { error = "Cannot create test user" });
                    }
                }

                // Create test song with Happy mood (ID = 1)
                var song = await _songService.CreateSongAsync(
                    "Test Song",
                    "Test Artist", 
                    "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    user.Id,
                    new List<int> { 1, 3 } // Happy and Relaxed moods
                );

                return Ok(new {
                    message = "Test song created successfully",
                    songId = song.Id,
                    title = song.Title,
                    artist = song.Artist,
                    moods = song.SongMoods.Select(sm => sm.Mood.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-playlist")]
        public async Task<IActionResult> CreateTestPlaylist()
        {
            try
            {
                // Try to get test user
                var user = await _authService.LoginAsync("testuser2", "password123");
                if (user == null)
                {
                    return BadRequest(new { error = "Test user not found. Create test song first." });
                }

                // Generate playlist for Happy mood (ID = 1)
                var playlist = await _playlistService.GeneratePlaylistAsync(
                    user.Id, 
                    1, // Happy mood
                    5, 
                    "Test Playlist"
                );

                return Ok(new {
                    message = "Test playlist created successfully",
                    playlistId = playlist.Id,
                    name = playlist.Name,
                    mood = playlist.Mood.Name,
                    songCount = playlist.PlaylistSongs.Count,
                    songs = playlist.PlaylistSongs
                        .OrderBy(ps => ps.Position)
                        .Select(ps => new { 
                            title = ps.Song.Title,
                            artist = ps.Song.Artist,
                            position = ps.Position
                        })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("database-info")]
        public IActionResult GetDatabaseInfo()
        {
            try
            {
                var userCount = _context.Users.Count();
                var songCount = _context.Songs.Count();
                var moodCount = _context.Moods.Count();
                var playlistCount = _context.Playlists.Count();

                return Ok(new {
                    users = userCount,
                    songs = songCount,
                    moods = moodCount,
                    playlists = playlistCount,
                    databaseLocation = "MoodPlaylist.db"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
