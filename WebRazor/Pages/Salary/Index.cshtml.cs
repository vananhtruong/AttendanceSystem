using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class SalaryIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<SalaryRecordDTO> SalaryRecords { get; set; } = new();

    public SalaryIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        int userId = 0;
        if (User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedId))
            {
                userId = parsedId;
            }
        }
        if (userId == 0)
        {
            SalaryRecords = new();
            return;
        }
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        SalaryRecords = await client.GetFromJsonAsync<List<SalaryRecordDTO>>($"api/Salary/user/{userId}") ?? new();
    }
} 