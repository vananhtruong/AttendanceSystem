using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<List<AttendanceRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.AttendanceRecords.Include(a => a.User).Include(a => a.WorkSchedule).Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetAllAsync()
        {
            return await _context.AttendanceRecords.Include(a => a.User).Include(a => a.WorkSchedule).ToListAsync();
        }

        public async Task AddAsync(AttendanceRecord record)
        {
            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();
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
    }
} 