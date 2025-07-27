using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs
{
    public class BulkWorkScheduleCreateDTO
    {
        public List<WorkScheduleCreateDTO> Dtos { get; set; } = new();
        public DateTime? StartDate { get; set; }   // ngày bắt đầu
        public DateTime? EndDate { get; set; }     // ngày kết thúc
    }
}
