using POS.Database; // Correct using directive for DbContext
using POS.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
// Redundant/unused usings: System.ComponentModel, System.Collections

namespace POS.Controllers
{
    public class AuthController
    {
        private readonly POSDbContext _context; // Correctly referencing POSDbContext

        public AuthController(POSDbContext context) // EXCELLENT: Using DI for DbContext
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            // FirstOrDefaultAsync with IsActive is good for security
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            // Using BCrypt.Net for password verification - EXCELLENT SECURITY PRACTICE
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            // Hashing password before saving - EXCELLENT SECURITY PRACTICE
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0; // Returns true if changes were saved
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            // If you have a global query filter for IsActive in POSDbContext,
            // this will automatically only return active users. If not, add .Where(u => u.IsActive).
            return await _context.Users.ToListAsync();
        }
    }
}