using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace WebRazor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var exp = jwtToken.ValidTo;

            // Hết hạn
            if (exp < DateTime.UtcNow)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            // Lấy role
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";

            // Điều hướng theo role
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/DashboardAdmin");
            if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/DashboardEmployee");

            // Mặc định
            return RedirectToPage("/Dashboard/Index");
        }
    }
}
