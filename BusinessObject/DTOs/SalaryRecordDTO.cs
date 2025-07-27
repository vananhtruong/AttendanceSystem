namespace BusinessObject.DTOs
{
    public class SalaryRecordDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public DateTime Month { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
    }
} 