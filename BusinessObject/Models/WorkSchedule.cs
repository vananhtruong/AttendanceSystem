using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class WorkSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int WorkShiftId { get; set; }
        public WorkShift WorkShift { get; set; }

        public DateTime WorkDate { get; set; }

        public string Status { get; set; } // "Absent", "Completed", "Insufficient", "NotYet"
    }


}
