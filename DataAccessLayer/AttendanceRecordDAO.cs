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

        public async Task<AttendanceRecord?> GetByIdAsync(int id)
        {
            return await _context.AttendanceRecords.Include(a => a.User).Include(a => a.WorkSchedule).FirstOrDefaultAsync(a => a.Id == id);
        }
        
        public async Task AddAsync(AttendanceRecord record)
        {
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AttendanceRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.AttendanceRecords.Include(a => a.User).Include(a => a.WorkSchedule).Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetAllAsync()
        {
          return await _context.AttendanceRecords.Include(a => a.User).Include(a => a.WorkSchedule).ToListAsync();
        }
        
         public async Task UpdateAsync(AttendanceRecord record)
        {
            _context.AttendanceRecords.Update(record);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record != null)
            {
                _context.AttendanceRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
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
    }
} 