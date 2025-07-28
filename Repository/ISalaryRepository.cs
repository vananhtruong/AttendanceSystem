using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface ISalaryRepository
    {
        Task<SalaryRecord?> GetByIdAsync(int id);
        Task<SalaryRecord?> GetByUserIdAndMonthAsync(int userId, DateTime month);
        Task<List<SalaryRecord>> GetAllAsync();
        Task AddAsync(SalaryRecord salary);
        Task UpdateAsync(SalaryRecord salary);
        Task DeleteAsync(int id);
        Task<SalaryRecord> CalculateSalaryAsync(int userId, DateTime month);
    }
}
