using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs
{
    public class WorkScheduleBulkCreateDTO
    {
        public List<int> UserIds { get; set; } = new();
        public int WorkShiftId { get; set; }
        public DateTime WorkDate { get; set; }
        public string Status { get; set; }
    }
}
