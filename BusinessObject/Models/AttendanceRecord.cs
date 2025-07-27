namespace BusinessObject.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public DateTime Date { get; set; }
        public string Status { get; set; } // "OnTime", "Late", "Absent", v.v.

        public int? WorkScheduleId { get; set; }
        public WorkSchedule WorkSchedule { get; set; }

        public decimal HoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
    }
}
