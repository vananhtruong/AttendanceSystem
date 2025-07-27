using BusinessObject.Models;
using DataAccessLayer;

namespace Repository
{
    public class AttendanceRecordRepository : IAttendanceRecordRepository
    {
        private readonly AttendanceRecordDAO _dao;
        public AttendanceRecordRepository(AttendanceRecordDAO dao)
        {
            _dao = dao;
        }
        public Task<AttendanceRecord?> GetByIdAsync(int id) => _dao.GetByIdAsync(id);
        public Task AddAsync(AttendanceRecord record) => _dao.AddAsync(record);
        public Task UpdateAsync(AttendanceRecord record) => _dao.UpdateAsync(record);
        public Task<List<AttendanceRecord>> GetByUserIdAsync(int userId) => _dao.GetByUserIdAsync(userId);
        public Task<List<AttendanceRecord>> GetAllAsync() => _dao.GetAllAsync();
        public Task<List<AttendanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) => _dao.GetByDateRangeAsync(startDate, endDate);
        public Task<AttendanceRecord?> GetByUserIdAndDateAsync(int userId, DateTime date, string type) => _dao.GetByUserIdAndDateAsync(userId,date,type);
        public Task DeleteAsync(int id) => _dao.DeleteAsync(id);
    }
} 