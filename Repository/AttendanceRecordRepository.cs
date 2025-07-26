using BusinessObject.Models;
using DataAccessLayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class AttendanceRecordRepository : IAttendanceRecordRepository
    {
        private readonly AttendanceRecordDAO _attendanceRecordDAO;

        public AttendanceRecordRepository(AttendanceRecordDAO attendanceRecordDAO)
        {
            _attendanceRecordDAO = attendanceRecordDAO;
        }

        public Task<AttendanceRecord?> GetByIdAsync(int id)
            => _attendanceRecordDAO.GetByIdAsync(id);
        public Task<List<AttendanceRecord>> GetByUserIdAsync(int userId)
            => _attendanceRecordDAO.GetByUserIdAsync(userId);
        public Task<List<AttendanceRecord>> GetAllAsync()
            => _attendanceRecordDAO.GetAllAsync();
        public Task AddAsync(AttendanceRecord record)
            => _attendanceRecordDAO.AddAsync(record);
        public Task UpdateAsync(AttendanceRecord record)
            => _attendanceRecordDAO.UpdateAsync(record);
        public Task DeleteAsync(int id)
            => _attendanceRecordDAO.DeleteAsync(id);
    }
} 