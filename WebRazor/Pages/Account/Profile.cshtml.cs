using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace WebRazor.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        public void OnGet()
        {
            // Page logic can be added here if needed
        }
    }
} 