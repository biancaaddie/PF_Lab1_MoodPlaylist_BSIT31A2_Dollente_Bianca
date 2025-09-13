using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MoodPlaylistGenerator.Services;
using MoodPlaylistGenerator.ViewModels;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class PlaylistsController : Controller
    {
        private readonly PlaylistService _playlistService;
        private readonly SongService _songService;

        public PlaylistsController(PlaylistService playlistService, SongService songService)
        {
            _playlistService = playlistService;
            _songService = songService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        public async Task<IActionResult> Index(int? moodId)
        {
            var userId = GetCurrentUserId();
            var playlists = await _playlistService.GetUserPlaylistsAsync(userId);
            var moods = await _songService.GetAllMoodsAsync();

            // Filter by mood if selected
            if (moodId.HasValue)
            {
                playlists = playlists.Where(p => p.MoodId == moodId.Value).ToList();
            }

            var viewModel = new PlaylistListViewModel
            {
                Playlists = playlists,
                Moods = moods,
                FilterMoodId = moodId
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id, userId);
            
            if (playlist == null)
                return NotFound();

            var viewModel = new PlaylistDetailViewModel
            {
                Playlist = playlist,
                Songs = playlist.PlaylistSongs.OrderBy(ps => ps.Position).ToList(),
                CanEdit = true
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var userId = GetCurrentUserId();
            var moods = await _songService.GetAllMoodsAsync();
            var songCounts = await _playlistService.GetMoodSongCountsAsync(userId);

            var viewModel = new GeneratePlaylistViewModel
            {
                AvailableMoods = moods,
                MoodSongCounts = songCounts
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(GeneratePlaylistViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.MoodSongCounts = await _playlistService.GetMoodSongCountsAsync(userId);
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var playlist = await _playlistService.GeneratePlaylistAsync(
                    userId, 
                    model.SelectedMoodId, 
                    model.SongCount, 
                    model.PlaylistName);

                TempData["SuccessMessage"] = "Playlist generated successfully!";
                return RedirectToAction(nameof(Details), new { id = playlist.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var userId = GetCurrentUserId();
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.MoodSongCounts = await _playlistService.GetMoodSongCountsAsync(userId);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while generating the playlist.");
                var userId = GetCurrentUserId();
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                model.MoodSongCounts = await _playlistService.GetMoodSongCountsAsync(userId);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateName(EditPlaylistNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid playlist name.";
                return RedirectToAction(nameof(Details), new { id = model.PlaylistId });
            }

            var userId = GetCurrentUserId();
            var playlist = await _playlistService.UpdatePlaylistNameAsync(model.PlaylistId, userId, model.Name);
            
            if (playlist == null)
                return NotFound();

            TempData["SuccessMessage"] = "Playlist name updated successfully!";
            return RedirectToAction(nameof(Details), new { id = model.PlaylistId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _playlistService.DeletePlaylistAsync(id, userId);
            
            if (!success)
                return NotFound();

            TempData["SuccessMessage"] = "Playlist deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
