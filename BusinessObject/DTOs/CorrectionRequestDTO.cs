namespace BusinessObject.DTOs
{
    public class CorrectionRequestDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int? AttendanceRecordId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public string RejectReason { get; set; }
        public string Status { get; set; }
        public string? EvidenceImageUrl { get; set; }
    }
} 