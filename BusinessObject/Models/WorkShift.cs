using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class WorkShift
    {
        public int Id { get; set; }
        public string Name { get; set; } // "Ca sáng", "Ca chiều"
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOvertime { get; set; }
    }
}
