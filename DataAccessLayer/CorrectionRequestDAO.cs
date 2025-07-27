using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class CorrectionRequestDAO
    {
        private readonly MyDbContext _context;

        public CorrectionRequestDAO(MyDbContext context)
        {
            _context = context;
        }

        public async Task<CorrectionRequest?> GetByIdAsync(int id)
        {
            return await _context.CorrectionRequests
                .Include(cr => cr.User)
                .Include(cr => cr.AttendanceRecord)
                .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        public async Task<List<CorrectionRequest>> GetByUserIdAsync(int userId)
        {
            return await _context.CorrectionRequests
                .Include(cr => cr.User)
                .Include(cr => cr.AttendanceRecord)
                .Where(cr => cr.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<CorrectionRequest>> GetAllAsync()
        {
            return await _context.CorrectionRequests
                .Include(cr => cr.User)
                .Include(cr => cr.AttendanceRecord)
                .ToListAsync();
        }

        public async Task AddAsync(CorrectionRequest request)
        {
            await _context.CorrectionRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CorrectionRequest request)
        {
            _context.CorrectionRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.CorrectionRequests.FindAsync(id);
            if (request != null)
            {
                _context.CorrectionRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
} 