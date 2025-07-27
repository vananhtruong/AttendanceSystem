using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Models;

namespace Repository
{
    public interface IWorkScheduleRepository
    {
        Task<WorkSchedule?> GetByIdAsync(int id);
        Task<List<WorkSchedule>> GetByUserIdAsync(int userId);
        Task<List<WorkSchedule>> GetAllAsync();
        Task AddAsync(WorkSchedule schedule);
        Task UpdateAsync(WorkSchedule schedule);
        Task DeleteAsync(int id);
        Task<List<WorkSchedule>> GetOvertimeSchedulesAsync(int userId);
        Task UpdateAttendanceStatusAsync(int scheduleId, string status);
        Task AddRangeAsync(List<WorkSchedule> schedules);
        Task<bool> ExistsAsync(int userId, DateTime workDate, int workShiftId);

    }
}
