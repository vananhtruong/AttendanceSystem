using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

public class WorkScheduleIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<WorkScheduleWithAttendanceDTO> WorkSchedules { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string SelectedWeek { get; set; }
    public string SelectedWeekString => SelectedWeek ?? DateTime.Today.ToString("yyyy-'W'ww");
    public string WeekRangeString { get; set; }
    public DateTime WeekStart { get; set; }

    public WorkScheduleIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        int userId = 0;
        if (User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedId))
            {
                userId = parsedId;
            }
        }
        if (userId == 0)
        {
            WorkSchedules = new();
            WeekStart = GetWeekStart(SelectedWeek);
            WeekRangeString = $"{WeekStart:dd/MM/yyyy} - {WeekStart.AddDays(6):dd/MM/yyyy}";
            return;
        }
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var allSchedules = await client.GetFromJsonAsync<List<WorkScheduleWithAttendanceDTO>>($"api/WorkSchedule/user/{userId}/with-attendance") ?? new();

        WeekStart = GetWeekStart(SelectedWeek);
        var weekEnd = WeekStart.AddDays(6);
        WorkSchedules = allSchedules.Where(x => x.WorkDate.Date >= WeekStart && x.WorkDate.Date <= weekEnd).ToList();
        WeekRangeString = $"{WeekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";
    }

    // Hàm tính ngày đầu tuần (thứ 2) từ SelectedWeek dạng yyyy-Www
    private DateTime GetWeekStart(string selectedWeek)
    {
        if (string.IsNullOrEmpty(selectedWeek))
            return DateTime.Today.StartOfWeek(DayOfWeek.Monday);

        var parts = selectedWeek.Split("-W");
        if (parts.Length != 2) return DateTime.Today.StartOfWeek(DayOfWeek.Monday);
        int year = int.Parse(parts[0]);
        int week = int.Parse(parts[1]);

        // ISO 8601: tuần 1 là tuần có ngày 4/1
        var jan4 = new DateTime(year, 1, 4);
        int daysOffset = DayOfWeek.Monday - jan4.DayOfWeek;
        var firstMonday = jan4.AddDays(daysOffset);
        return firstMonday.AddDays((week - 1) * 7);
    }
}

// Extension cho DateTime
public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}