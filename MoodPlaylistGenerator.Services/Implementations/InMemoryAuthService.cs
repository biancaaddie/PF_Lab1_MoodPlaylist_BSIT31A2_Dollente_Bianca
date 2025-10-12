using MoodPlaylistGenerator.Data.Entities;
using MoodPlaylistGenerator.Services.Interfaces;
using BCrypt.Net;

namespace MoodPlaylistGenerator.Services.Implementations
{
    /// <summary>
    /// In-memory implementation of IAuthService for learning and testing purposes.
    /// This implementation uses a simple List to store users instead of a database.
    /// Perfect for students to understand the service pattern without database complexity.
    /// </summary>
    public class InMemoryAuthService : IAuthService
    {
        private readonly List<User> _users;
        private int _nextUserId = 1;

        public InMemoryAuthService()
        {
            _users = new List<User>();
            
            // Seed with some default users for testing
            // Password for both users is "password123"
            _users.Add(new User
            {
                Id = _nextUserId++,
                Email = "admin@test.com",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow
            });
            
            _users.Add(new User
            {
                Id = _nextUserId++,
                Email = "user@test.com",
                Username = "testuser", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<User?> RegisterAsync(string email, string username, string password)
        {
            // Simulate async operation
            await Task.Delay(1);
            
            // Check if user already exists
            var existingUser = _users.FirstOrDefault(u => u.Email == email || u.Username == username);
            if (existingUser != null)
                return null; // User already exists

            // Create new user with hashed password
            var user = new User
            {
                Id = _nextUserId++,
                Email = email,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            // Add to our in-memory list
            _users.Add(user);
            return user;
        }

        public async Task<User?> LoginAsync(string emailOrUsername, string password)
        {
            // Simulate async operation
            await Task.Delay(1);
            
            // Find user by email or username
            var user = _users.FirstOrDefault(u => 
                u.Email == emailOrUsername || u.Username == emailOrUsername);

            // Verify password
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // Update last login time
            user.LastLogin = DateTime.UtcNow;
            return user;
        }

        public async Task<bool> InitiatePasswordResetAsync(string email)
        {
            // Simulate async operation
            await Task.Delay(1);
            
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return false;

            // Generate reset token (in real app, this would be sent via email)
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            // For demonstration, log the token (in real app, send email)
            Console.WriteLine($"Password reset token for {email}: {user.ResetToken}");
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // Simulate async operation
            await Task.Delay(1);
            
            // Find user with valid reset token
            var user = _users.FirstOrDefault(u => 
                u.ResetToken == token && 
                u.ResetTokenExpiry != null && 
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return false;

            // Update password and clear reset token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            return true;
        }

        /// <summary>
        /// Helper method for debugging - shows all users in memory
        /// </summary>
        public List<User> GetAllUsers() => _users.ToList();

        /// <summary>
        /// Helper method to clear all users (useful for testing)
        /// </summary>
        public void ClearAllUsers() => _users.Clear();
    }
}
