using BusinessObject.Models;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class WorkShiftRepository : IWorkShiftRepository
    {
        private readonly WorkShiftDAO _dao;
        public WorkShiftRepository(WorkShiftDAO dao)
        {
            _dao = dao;
        }

        public Task<List<WorkShift>> GetAllAsync() => _dao.GetAllAsync();
        public Task<WorkShift> GetByIdAsync(int id) => _dao.GetByIdAsync(id);
        public Task<WorkShift> CreateAsync(WorkShift shift) => _dao.CreateAsync(shift);
        public Task UpdateAsync(WorkShift shift) => _dao.UpdateAsync(shift);
        public Task DeleteAsync(int id) => _dao.DeleteAsync(id);
    }
}
