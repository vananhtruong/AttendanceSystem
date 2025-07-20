using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        // FK
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
