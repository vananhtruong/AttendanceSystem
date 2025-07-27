using BusinessObject.DTOs;
using BusinessObject.Models;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class WorkScheduleRepository : IWorkScheduleRepository
    {
        private readonly WorkScheduleDAO _workScheduleDAO;

        public WorkScheduleRepository(WorkScheduleDAO workScheduleDAO)
        {
            _workScheduleDAO = workScheduleDAO;
        }

        public Task<WorkSchedule?> GetByIdAsync(int id)
            => _workScheduleDAO.GetByIdAsync(id);
        public Task<List<WorkSchedule>> GetByUserIdAsync(int userId)
            => _workScheduleDAO.GetByUserIdAsync(userId);
        public Task<List<WorkSchedule>> GetAllAsync()
            => _workScheduleDAO.GetAllAsync();
        public Task AddAsync(WorkSchedule schedule)
            => _workScheduleDAO.AddAsync(schedule);
        public Task UpdateAsync(WorkSchedule schedule)
            => _workScheduleDAO.UpdateAsync(schedule);
        public Task DeleteAsync(int id)
            => _workScheduleDAO.DeleteAsync(id);
        public Task<List<WorkSchedule>> GetOvertimeSchedulesAsync(int userId)
            => _workScheduleDAO.GetOvertimeSchedulesAsync(userId);
        public Task UpdateAttendanceStatusAsync(int scheduleId, string status)
            => _workScheduleDAO.UpdateAttendanceStatusAsync(scheduleId, status);

    }
}
