using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "Employee"; // hoặc "Admin"

        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
