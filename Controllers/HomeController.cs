using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MoodPlaylistGenerator.Models;
using MoodPlaylist.SQLite.Services;
using MoodPlaylistGenerator.ViewModels;

namespace MoodPlaylistGenerator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SongService? _songService;
    private readonly PlaylistService? _playlistService;

    public HomeController(ILogger<HomeController> logger, SongService? songService = null, PlaylistService? playlistService = null)
    {
        _logger = logger;
        _songService = songService;
        _playlistService = playlistService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true && _songService != null && _playlistService != null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var allSongs = await _songService.GetUserSongsAsync(userId);
            var allPlaylists = await _playlistService.GetUserPlaylistsAsync(userId);
            var moods = await _songService.GetAllMoodsAsync();
            var songCounts = await _playlistService.GetMoodSongCountsAsync(userId);

            var dashboardModel = new DashboardViewModel
            {
                RecentSongs = allSongs.Take(5).ToList(),
                RecentPlaylists = allPlaylists.Take(5).ToList(),
                Moods = moods,
                MoodSongCounts = songCounts,
                TotalSongs = allSongs.Count,
                TotalPlaylists = allPlaylists.Count
            };

            return View("Dashboard", dashboardModel);
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
