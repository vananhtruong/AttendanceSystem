using BusinessObject.Models;

namespace BusinessObject.DTOs
{
    public class WorkScheduleWithAttendanceDTO
    {
        public int WorkScheduleId { get; set; }
        public DateTime WorkDate { get; set; }
        public string ShiftName { get; set; }
        public bool IsOvertime { get; set; }
        public string ScheduleStatus { get; set; }
        public string AttendanceStatus { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int WorkShiftId { get; set; }
        public WorkShift WorkShift { get; set; }
        public string Status { get; set; }
    }
} 