using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? WorkScheduleId { get; set; }
        public WorkSchedule WorkSchedule { get; set; }

        public DateTime RecordTime { get; set; } // Time Quét
        public string Type { get; set; } // "CheckIn" or "CheckOut"
        public string Status { get; set; } // "OnTime", "Late", "NotYet", "Absent"

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursWorked { get; set; } 

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; }
    }
}
