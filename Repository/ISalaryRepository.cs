using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Models;

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
