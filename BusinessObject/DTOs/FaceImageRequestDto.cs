using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs
{
    public class FaceImageRequestDto
    {
        [Required]
        public IFormFile FaceImage { get; set; }
    }
} 