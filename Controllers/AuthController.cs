using POS.Database;
using POS.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using POS.Database;
using System.Collections;

namespace POS.Controllers
{
    public class AuthController
    {
        private readonly POSDbContext _context;

        public AuthController(POSDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
