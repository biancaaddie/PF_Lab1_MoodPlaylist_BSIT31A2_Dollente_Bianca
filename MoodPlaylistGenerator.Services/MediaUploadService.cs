using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services
{
    public class MediaUploadService : IMediaUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        
        // 50 MB default limit (can be overridden in appsettings)
        private const long DefaultMaxFileSizeBytes = 50L * 1024L * 1024L;
        private static readonly string[] AllowedAudioExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".flac" };
        private static readonly string[] AllowedVideoExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov", ".mkv", ".avi" };

        public MediaUploadService(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        
        public async Task<(string filePath, string fileName, string contentType, long fileSize)> SaveMediaFileAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            if (!IsValidMediaFile(file))
                throw new InvalidOperationException("Unsupported media type or file too large.");

            var uploadPath = _configuration["MediaUpload:UploadPath"] ?? "wwwroot/uploads/media";
            var fullUploadPath = Path.Combine(_environment.ContentRootPath, uploadPath);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(fullUploadPath);
            
            // Create user-specific subdirectory
            var userDirectory = Path.Combine(fullUploadPath, $"user_{userId}");
            Directory.CreateDirectory(userDirectory);
            
            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(userDirectory, uniqueFileName);
            
            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Return relative path for database storage
            var relativePath = Path.Combine(uploadPath, $"user_{userId}", uniqueFileName);
            
            return (relativePath, uniqueFileName, file.ContentType, file.Length);
        }
        
        public bool IsValidMediaFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;
                
            var maxSizeMB = _configuration.GetValue<int>("MediaUpload:MaxFileSizeMB", 50);
            if (file.Length > maxSizeMB * 1024 * 1024)
                return false;
                
            var allowedAudio = _configuration.GetSection("MediaUpload:AllowedAudioTypes").Get<string[]>() ?? Array.Empty<string>();
            var allowedVideo = _configuration.GetSection("MediaUpload:AllowedVideoTypes").Get<string[]>() ?? Array.Empty<string>();
            
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedAudio.Contains(fileExtension) || allowedVideo.Contains(fileExtension);
        }

        public bool FileExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            return File.Exists(fullPath);
        }

        public void DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public string GetMediaUrl(string filePath)
        {
            return "/" + filePath.Replace("\\", "/").Replace("wwwroot/", "");
        }
        
        public string GetFallbackYouTubeUrl()
        {
            return _configuration["MediaUpload:FallbackYouTubeUrl"] ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        }

        public MediaType DetermineMediaType(string contentType)
        {
            if (contentType.StartsWith("audio/"))
                return MediaType.LocalAudio;
            else if (contentType.StartsWith("video/"))
                return MediaType.LocalVideo;
            else
                return MediaType.YouTube;
        }
    }
}
