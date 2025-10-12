namespace MoodPlaylistGenerator.Services
{
    public interface IMediaUploadService
    {
        // TODO: Define method signature for saving uploaded files
        // Task<(string filePath, string fileName, string contentType, long fileSize)> SaveMediaFileAsync(IFormFile file, int userId);
        
        // TODO: Add method to validate media files
        // bool IsValidMediaFile(IFormFile file);
        
        // TODO: Add method to check if file exists
        // bool FileExists(string filePath);
        
        // TODO: Add method to get media URL for playback
        // string GetMediaUrl(string filePath);
        
        // TODO: Add method to get Rick Roll fallback URL
        string GetFallbackYouTubeUrl();
    }
}
