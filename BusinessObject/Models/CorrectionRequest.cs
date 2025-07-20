﻿namespace BusinessObject.Models
{
    public class CorrectionRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime Date { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; } // "Pending", "Approved", "Rejected"
    }
}
