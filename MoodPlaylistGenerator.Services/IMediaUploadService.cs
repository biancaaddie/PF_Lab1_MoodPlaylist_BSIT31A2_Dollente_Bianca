using Microsoft.AspNetCore.Http;
using MoodPlaylistGenerator.Data.Entities;

namespace MoodPlaylistGenerator.Services
{
    public interface IMediaUploadService
    {
        Task<(string filePath, string fileName, string contentType, long fileSize)> SaveMediaFileAsync(IFormFile file, int userId);
        bool IsValidMediaFile(IFormFile file);
        bool FileExists(string? filePath);
        void DeleteFile(string filePath);
        string GetMediaUrl(string filePath);
        string GetFallbackYouTubeUrl();
        MediaType DetermineMediaType(string contentType);
    }
}
