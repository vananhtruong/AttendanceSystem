using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class WorkScheduleDAO
    {
        private readonly MyDbContext _context;

        public WorkScheduleDAO(MyDbContext context)
        {
            _context = context;
        }

        public async Task<WorkSchedule?> GetByIdAsync(int id)
        {
            return await _context.WorkSchedules.Include(w => w.User).Include(w => w.WorkShift).FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<WorkSchedule>> GetByUserIdAsync(int userId)
        {
            return await _context.WorkSchedules.Include(w => w.User).Include(w => w.WorkShift).Where(w => w.UserId == userId).ToListAsync();
        }

        public async Task<List<WorkSchedule>> GetAllAsync()
        {
            return await _context.WorkSchedules.Include(w => w.User).Include(w => w.WorkShift).ToListAsync();
        }

        public async Task AddAsync(WorkSchedule schedule)
        {
            await _context.WorkSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkSchedule schedule)
        {
            _context.WorkSchedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.WorkSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<WorkSchedule>> GetOvertimeSchedulesAsync(int userId)
        {
            // Giả sử WorkShift có thuộc tính IsOvertime
            return await _context.WorkSchedules.Include(w => w.WorkShift)
                .Where(w => w.UserId == userId && w.WorkShift.IsOvertime)
                .ToListAsync();
        }

        public async Task UpdateAttendanceStatusAsync(int scheduleId, string status)
        {
            var schedule = await _context.WorkSchedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                // Giả sử WorkSchedule có thuộc tính Status
                schedule.GetType().GetProperty("Status")?.SetValue(schedule, status);
                await _context.SaveChangesAsync();
            }
        }
    }
}
