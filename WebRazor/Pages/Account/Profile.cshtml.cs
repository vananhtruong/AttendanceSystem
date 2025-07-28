using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using WebRazor.Services;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebRazor.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IApiService _apiService;

        public ProfileModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public void OnGet()
        {
            // Page logic will be handled by JavaScript
        }

        public async Task<IActionResult> OnGetFaceImageAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized();
                }

                // Call the WebAPI to get face image
                var response = await _apiService.GetRawAsync("api/faceattendance/my-face-image", token);
                
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(imageBytes, "image/jpeg");
                }
                
                return NotFound();
            }
            catch
            {
                return NotFound();
            }
        }
    }
} 