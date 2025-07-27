using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs
{
    public class RegisterUserRequestDto : FaceImageRequestDto
    {
        [Required]
        public int UserId { get; set; }
    }
} 