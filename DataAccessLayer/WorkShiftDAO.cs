using BusinessObject.Models;
using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class WorkShiftDAO
    {
        private readonly MyDbContext _context;
        public WorkShiftDAO(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkShift>> GetAllAsync()
        {
            return await _context.WorkShifts.ToListAsync();
        }

        public async Task<WorkShift> GetByIdAsync(int id)
        {
            return await _context.WorkShifts.FindAsync(id);
        }

        public async Task<WorkShift> CreateAsync(WorkShift shift)
        {
            _context.WorkShifts.Add(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task UpdateAsync(WorkShift shift)
        {
            _context.WorkShifts.Update(shift);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var shift = await _context.WorkShifts.FindAsync(id);
            if (shift != null)
            {
                _context.WorkShifts.Remove(shift);
                await _context.SaveChangesAsync();
            }
        }
    }
}
