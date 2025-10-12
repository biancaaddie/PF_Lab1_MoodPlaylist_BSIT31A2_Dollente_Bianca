using MoodPlaylistGenerator.Models;

namespace MoodPlaylistGenerator.Services
{
    public interface IAuthService
    {
        Task<bool> InitiatePasswordResetAsync(string email);
        Task<User?> LoginAsync(string emailOrUsername, string password);
        Task<User?> RegisterAsync(string email, string username, string password);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }

    public class MsAccessAuthService: IAuthService
    {
        public Task<User?> RegisterAsync(string email, string username, string password)
        {
            throw new NotImplementedException();
        }
        public Task<User?> LoginAsync(string emailOrUsername, string password)
        {
            throw new NotImplementedException();
        }
        public Task<bool> InitiatePasswordResetAsync(string email)
        {
            throw new NotImplementedException();
        }
        public Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}