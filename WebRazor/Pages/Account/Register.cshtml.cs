using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty]
    public string FullName { get; set; }
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public string ErrorMessage { get; set; }

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("api");
        var response = await client.PostAsJsonAsync("api/Account/register", new { FullName, Email, Password });
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Account/Login");
        }
        ErrorMessage = "Registration failed. Please check your information.";
        return Page();
    }
} 