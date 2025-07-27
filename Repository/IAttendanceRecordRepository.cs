using BusinessObject.Models;

namespace Repository
{
    public interface IAttendanceRecordRepository
    {
        Task<AttendanceRecord?> GetByIdAsync(int id);
        Task AddAsync(AttendanceRecord record);
        Task UpdateAsync(AttendanceRecord record);
        Task<List<AttendanceRecord>> GetByUserIdAsync(int userId);
        Task<List<AttendanceRecord>> GetAllAsync();
        Task<List<AttendanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AttendanceRecord?> GetByUserIdAndDateAsync(int userId, DateTime date);
        Task DeleteAsync(int id);
    }
} 