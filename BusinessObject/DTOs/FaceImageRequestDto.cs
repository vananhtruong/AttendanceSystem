using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs
{
    public class FaceImageRequestDto
    {
        public IFormFile? FaceImage { get; set; }
        public string? ImageData { get; set; } // Base64 encoded image data
    }
} 