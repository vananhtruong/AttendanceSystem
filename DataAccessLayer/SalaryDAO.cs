//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BusinessObject;
//using BusinessObject.Models;
//using Microsoft.EntityFrameworkCore;

//namespace DataAccessLayer
//{
//    public class SalaryDAO
//    {
//        private readonly MyDbContext _context;

//        public SalaryDAO(MyDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<SalaryRecord?> GetByIdAsync(int id)
//        {
//            return await _context.salaryRecords.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
//        }

//        public async Task<SalaryRecord?> GetByUserIdAndMonthAsync(int userId, DateTime month)
//        {
//            return await _context.salaryRecords.Include(s => s.User)
//                .FirstOrDefaultAsync(s => s.UserId == userId && s.Month.Year == month.Year && s.Month.Month == month.Month);
//        }

//        public async Task<List<SalaryRecord>> GetAllAsync()
//        {
//            return await _context.salaryRecords.Include(s => s.User).ToListAsync();
//        }

//        public async Task AddAsync(SalaryRecord salary)
//        {
//            await _context.salaryRecords.AddAsync(salary);
//            await _context.SaveChangesAsync();
//        }

//        public async Task UpdateAsync(SalaryRecord salary)
//        {
//            _context.salaryRecords.Update(salary);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteAsync(int id)
//        {
//            var salary = await _context.salaryRecords.FindAsync(id);
//            if (salary != null)
//            {
//                _context.salaryRecords.Remove(salary);
//                await _context.SaveChangesAsync();
//            }
//        }

//        // Tính lương tự động dựa vào AttendanceRecord (giả sử đã có bảng AttendanceRecord)
//        public async Task<SalaryRecord> CalculateSalaryAsync(int userId, DateTime month)
//        {
//            // Lấy dữ liệu chấm công của user trong tháng
//            var attendances = await _context.AttendanceRecords
//                .Where(a => a.UserId == userId && a.Date.Month == month.Month && a.Date.Year == month.Year)
//                .ToListAsync();

//            decimal totalHours = attendances.Sum(a =>
//                a.CheckOutTime.HasValue ? (decimal)(a.CheckOutTime.Value - a.CheckInTime).TotalHours : 0);
//            // Nếu chưa có overtime, để 0
//            decimal overtime = 0;
//            decimal baseSalary = 20000; // ví dụ, lấy từ user hoặc config
//            decimal overtimeRate = 1.5m;
//            decimal amount = baseSalary * totalHours + overtime * baseSalary * overtimeRate;

//            var salaryRecord = new SalaryRecord
//            {
//                UserId = userId,
//                Month = new DateTime(month.Year, month.Month, 1),
//                TotalHoursWorked = totalHours,
//                OvertimeHours = overtime,
//                Amount = amount,
//                IsPaid = false
//            };
//            await AddAsync(salaryRecord);
//            return salaryRecord;
//        }
//    }
//}
