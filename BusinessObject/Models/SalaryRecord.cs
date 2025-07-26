using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class SalaryRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
            
        public DateTime Month { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
    }

}
