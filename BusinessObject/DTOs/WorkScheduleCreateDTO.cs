using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs
{
    public class WorkScheduleCreateDTO
    {
        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "WorkShiftId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "WorkShiftId must be a positive integer")]
        public int WorkShiftId { get; set; }
        
        [Required(ErrorMessage = "WorkDate is required")]
        public DateTime WorkDate { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string Status { get; set; }
    }
}
