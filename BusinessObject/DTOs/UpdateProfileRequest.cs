using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs
{
    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 