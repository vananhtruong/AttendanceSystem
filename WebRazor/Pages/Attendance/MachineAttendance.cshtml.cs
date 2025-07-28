using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace WebRazor.Pages.Attendance
{
    [Authorize(Roles = "Admin")]
    public class MachineAttendanceModel : PageModel
    {
        public string AccessToken { get; set; }

        public void OnGet()
        {
            // Lấy token từ session và truyền vào ViewData
            AccessToken = HttpContext.Session.GetString("AccessToken") ?? "";
        }
    }
} 