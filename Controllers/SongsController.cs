using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MoodPlaylistGenerator.Services;
using MoodPlaylistGenerator.ViewModels;

namespace MoodPlaylistGenerator.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly SongService _songService;
        private readonly IMediaUploadService _mediaUploadService;

        public SongsController(SongService songService, IMediaUploadService mediaUploadService)
        {
            _songService = songService;
            _mediaUploadService = mediaUploadService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        public async Task<IActionResult> Index(int? moodId, string? search)
        {
            var userId = GetCurrentUserId();
            var songs = await _songService.GetUserSongsAsync(userId);
            var moods = await _songService.GetAllMoodsAsync();

            // Filter by mood if selected
            if (moodId.HasValue)
            {
                songs = await _songService.GetSongsByMoodAsync(moodId.Value, userId);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                songs = songs.Where(s => 
                    s.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new SongListViewModel
            {
                Songs = songs,
                Moods = moods,
                SelectedMoodId = moodId,
                SearchTerm = search ?? ""
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            string? localUrl = null;
            bool isVideo = false;
            bool fileExists = false;
            
            if (!string.IsNullOrWhiteSpace(song.LocalFilePath) && _mediaUploadService.FileExists(song.LocalFilePath))
            {
                localUrl = _mediaUploadService.GetMediaUrl(song.LocalFilePath!);
                fileExists = true;
                isVideo = song.MediaType == MoodPlaylistGenerator.Data.Entities.MediaType.LocalVideo;
            }

            var viewModel = new SongDetailViewModel
            {
                Song = song,
                YouTubeVideoId = string.IsNullOrWhiteSpace(song.YouTubeUrl) ? "" : _songService.ExtractYouTubeVideoId(song.YouTubeUrl!),
                AssignedMoods = song.SongMoods.Select(sm => sm.Mood).ToList(),
                LocalMediaUrl = localUrl,
                IsVideo = isVideo,
                FallbackYouTubeUrl = _mediaUploadService.GetFallbackYouTubeUrl(),
                FileExists = fileExists
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSongViewModel
            {
                AvailableMoods = await _songService.GetAllMoodsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                string? savedPath = null;
                string? savedName = null;

                if (model.MediaFile != null && model.MediaFile.Length > 0)
                {
                    if (!_mediaUploadService.IsValidMediaFile(model.MediaFile))
                    {
                        ModelState.AddModelError("MediaFile", "Unsupported file type or too large. Accepts audio/video only.");
                        model.AvailableMoods = await _songService.GetAllMoodsAsync();
                        return View(model);
                    }

                    var result = await _mediaUploadService.SaveMediaFileAsync(model.MediaFile, userId);
                    savedPath = result.filePath;
                    savedName = result.fileName;
                }

                // Prefer local media if uploaded; YouTubeUrl is optional
                await _songService.CreateSongAsync(
                    model.Title,
                    model.Artist,
                    model.YouTubeUrl ?? string.Empty,
                    userId,
                    model.SelectedMoodIds
                );

                // If we saved a media file, attach it to the created song
                // Reload the last created song by this user (simplest approach here)
                var songs = await _songService.GetUserSongsAsync(userId);
                var created = songs.FirstOrDefault(s => s.Title == model.Title && s.Artist == model.Artist);
                if (created != null && savedPath != null)
                {
                    created.LocalFilePath = savedPath;
                    created.FileName = savedName;
                    created.ContentType = model.MediaFile!.ContentType;
                    created.FileSizeBytes = model.MediaFile.Length;
                    created.MediaType = _mediaUploadService.DetermineMediaType(model.MediaFile.ContentType);
                    await HttpContext.RequestServices.GetRequiredService<MoodPlaylistGenerator.Data.ApplicationDbContext>().SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Song added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while adding the song.");
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var song = await _songService.GetSongByIdAsync(id, userId);
            
            if (song == null)
                return NotFound();

            var viewModel = new EditSongViewModel
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                YouTubeUrl = song.YouTubeUrl,
                SelectedMoodIds = song.SongMoods.Select(sm => sm.MoodId).ToList(),
                AvailableMoods = await _songService.GetAllMoodsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                var updatedSong = await _songService.UpdateSongAsync(
                    model.Id, 
                    userId, 
                    model.Title, 
                    model.Artist, 
                    model.YouTubeUrl ?? string.Empty, 
                    model.SelectedMoodIds);

                if (updatedSong == null)
                    return NotFound();

                // Handle optional media replacement
                if (model.MediaFile != null && model.MediaFile.Length > 0)
                {
                    if (!_mediaUploadService.IsValidMediaFile(model.MediaFile))
                    {
                        ModelState.AddModelError("MediaFile", "Unsupported file type or too large. Accepts audio/video only.");
                        model.AvailableMoods = await _songService.GetAllMoodsAsync();
                        return View(model);
                    }

                    var result = await _mediaUploadService.SaveMediaFileAsync(model.MediaFile, userId);
                    updatedSong.LocalFilePath = result.filePath;
                    updatedSong.FileName = result.fileName;
                    updatedSong.ContentType = model.MediaFile.ContentType;
                    updatedSong.FileSizeBytes = model.MediaFile.Length;
                    updatedSong.MediaType = _mediaUploadService.DetermineMediaType(model.MediaFile.ContentType);
                    await HttpContext.RequestServices.GetRequiredService<MoodPlaylistGenerator.Data.ApplicationDbContext>().SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Song updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the song.");
                model.AvailableMoods = await _songService.GetAllMoodsAsync();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _songService.DeleteSongAsync(id, userId);
            
            if (!success)
                return NotFound();

            TempData["SuccessMessage"] = "Song deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
