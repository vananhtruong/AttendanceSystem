namespace BusinessObject.DTOs
{
    public class WorkScheduleDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int WorkShiftId { get; set; }
        public string WorkShiftName { get; set; }
        public bool IsOvertime { get; set; }
        public DateTime WorkDate { get; set; }
        public string Status { get; set; }
    }
} 