using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class AttendanceRecordDAO
    {
        private readonly MyDbContext _context;

        public AttendanceRecordDAO(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AttendanceRecord record)
        {
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AttendanceRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.AttendanceRecords.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetAllAsync()
        {
            return await _context.AttendanceRecords
                .Include(ar => ar.User)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceRecords
                .Include(ar => ar.User)
                .Where(ar => ar.Date >= startDate && ar.Date <= endDate)
                .ToListAsync();
        }

        public async Task<AttendanceRecord?> GetByUserIdAndDateAsync(int userId, DateTime date)
        {
            return await _context.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.Date == date);
        }

        public async Task UpdateAsync(AttendanceRecord record)
        {
            _context.AttendanceRecords.Update(record);
            await _context.SaveChangesAsync();
        }
    }
} 