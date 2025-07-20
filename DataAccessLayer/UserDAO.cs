using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class UserDAO
    {
        private readonly MyDbContext _context;

        public UserDAO(MyDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
