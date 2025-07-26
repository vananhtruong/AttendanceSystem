using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class CorrectionRequestIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<CorrectionRequestDTO> CorrectionRequests { get; set; } = new();
    public List<AttendanceRecordDTO> EligibleAttendanceRecords { get; set; } = new();

    public CorrectionRequestIndexModel(IHttpClientFactory httpClientFactory)
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
            CorrectionRequests = new();
            return;
        }
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        CorrectionRequests = await client.GetFromJsonAsync<List<CorrectionRequestDTO>>($"api/CorrectionRequest/user/{userId}") ?? new();

        // Load attendance records for the user
        var allAttendance = await client.GetFromJsonAsync<List<AttendanceRecordDTO>>($"api/Attendance/user/{userId}") ?? new();
        EligibleAttendanceRecords = allAttendance
            .Where(a => (DateTime.UtcNow - a.Date).TotalDays <= 2)
            .ToList();
    }

    // TODO: OnPostAsync để gửi request chỉnh sửa (gửi ảnh)
}