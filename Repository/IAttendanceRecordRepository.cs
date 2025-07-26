using BusinessObject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IAttendanceRecordRepository
    {
        Task<AttendanceRecord?> GetByIdAsync(int id);
        Task<List<AttendanceRecord>> GetByUserIdAsync(int userId);
        Task<List<AttendanceRecord>> GetAllAsync();
        Task AddAsync(AttendanceRecord record);
        Task UpdateAsync(AttendanceRecord record);
        Task DeleteAsync(int id);
    }
} 