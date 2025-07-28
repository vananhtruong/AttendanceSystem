using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json.Serialization;
using System;
using WebRazor.Services;

public class LoginResponseDto
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }
}

public class LoginModel : PageModel
{
    private readonly IApiService _apiService;
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public string ErrorMessage { get; set; }

    public LoginModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var response = await _apiService.PostRawAsync("api/Account/login", new { Email, Password });
            
            Console.WriteLine($"API Response Status: {response.StatusCode}");
            Console.WriteLine($"API Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Email: {Email}, Password: {Password}");
                var raw = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API response: " + raw);
                
                var loginResult = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(raw);
                if (loginResult != null && !string.IsNullOrEmpty(loginResult.AccessToken))
                {
                    // Giải mã JWT để lấy role
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(loginResult.AccessToken);
                    string role = null;
                    foreach (var claim in jwt.Claims)
                    {
                        Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                        if (claim.Type == "role" || claim.Type.EndsWith("/role"))
                            role = claim.Value;
                    }

                    if (role == "Admin")
                    {
                        HttpContext.Session.SetString("AccessToken", loginResult.AccessToken);
                        return Redirect($"/DashboardAdmin?token={loginResult.AccessToken}");
                    }
                    else if (role == "Employee")
                    {
                        HttpContext.Session.SetString("AccessToken", loginResult.AccessToken);
                        return Redirect($"/DashboardEmployee?token={loginResult.AccessToken}");
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                ErrorMessage = $"Login failed: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during login: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            ErrorMessage = $"Connection error: {ex.Message}";
        }
        
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            ErrorMessage = "Invalid email or password.";
        }
        return Page();
    }
} 