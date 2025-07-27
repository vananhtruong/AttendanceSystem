using BusinessObject.Models;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly SalaryDAO _salaryDAO;

        public SalaryRepository(SalaryDAO salaryDAO)
        {
            _salaryDAO = salaryDAO;
        }

        public Task<SalaryRecord?> GetByIdAsync(int id)
            => _salaryDAO.GetByIdAsync(id);

        public Task<SalaryRecord?> GetByUserIdAndMonthAsync(int userId, DateTime month)
            => _salaryDAO.GetByUserIdAndMonthAsync(userId, month);

        public Task<List<SalaryRecord>> GetAllAsync()
            => _salaryDAO.GetAllAsync();

        public Task AddAsync(SalaryRecord salary)
            => _salaryDAO.AddAsync(salary);

        public Task UpdateAsync(SalaryRecord salary)
            => _salaryDAO.UpdateAsync(salary);

        public Task DeleteAsync(int id)
            => _salaryDAO.DeleteAsync(id);

        public Task<SalaryRecord> CalculateSalaryAsync(int userId, DateTime month)
            => _salaryDAO.CalculateSalaryAsync(userId, month);
    }
}
