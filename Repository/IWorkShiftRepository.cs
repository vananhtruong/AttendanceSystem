using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IWorkShiftRepository
    {
        Task<List<WorkShift>> GetAllAsync();
        Task<WorkShift> GetByIdAsync(int id);
        Task<WorkShift> CreateAsync(WorkShift shift);
        Task UpdateAsync(WorkShift shift);
        Task DeleteAsync(int id);
    }
}
