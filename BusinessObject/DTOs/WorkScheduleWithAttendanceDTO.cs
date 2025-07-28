using BusinessObject.Models;

namespace BusinessObject.DTOs
{
    public class WorkScheduleWithAttendanceDTO
    {
        public int WorkScheduleId { get; set; }
        public DateTime WorkDate { get; set; }
        public string ShiftName { get; set; }
        public bool IsOvertime { get; set; }

        // Trạng thái ca làm (Completed/Insufficient/Absent/NotYet)
        public string ScheduleStatus { get; set; }

        // Thời gian & trạng thái chấm công
        public DateTime? CheckInTime { get; set; }
        public string CheckInStatus { get; set; } // OnTime / Late / Absent

        public DateTime? CheckOutTime { get; set; }
        public string CheckOutStatus { get; set; } // OnTime / EarlyLeave / Absent

        // Tổng giờ làm
        public decimal HoursWorked { get; set; }

        // Thông tin ca làm
        public int WorkShiftId { get; set; }
        public WorkShift WorkShift { get; set; }

        // Để tiện hiển thị ca: giờ bắt đầu - giờ kết thúc
        public string ShiftStart => WorkShift != null ? WorkShift.StartTime.ToString(@"hh\:mm") : "--";
        public string ShiftEnd => WorkShift != null ? WorkShift.EndTime.ToString(@"hh\:mm") : "--";
    }
}
