namespace BusinessObject.DTOs
{
    public class AttendanceRecordDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int? WorkScheduleId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
    }
} 