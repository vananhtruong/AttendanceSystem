using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using System.Text.Json.Serialization;
public class LoginResponseDto
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }
}

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public string ErrorMessage { get; set; }

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("api");
        var response = await client.PostAsJsonAsync("api/Account/login", new { Email, Password });
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

                // (Optional) Lưu token vào localStorage qua JS interop nếu muốn
                // await JS.InvokeVoidAsync("localStorage.setItem", "accessToken", loginResult.AccessToken);

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
        ErrorMessage = "Invalid email or password.";
        return Page();
    }
} 