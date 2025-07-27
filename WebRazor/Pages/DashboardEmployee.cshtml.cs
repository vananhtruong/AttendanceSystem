using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;

public class DashboardEmployeeModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public DashboardDataDTO DashboardData { get; set; } = new();

    public DashboardEmployeeModel(IHttpClientFactory httpClientFactory)
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
            DashboardData = new();
            return;
        }
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var schedules = await client.GetFromJsonAsync<List<WorkScheduleWithAttendanceDTO>>($"api/WorkSchedule/user/{userId}/with-attendance") ?? new();
        var salary = await client.GetFromJsonAsync<List<SalaryRecordDTO>>($"api/Salary/user/{userId}") ?? new();
        var notifications = await client.GetFromJsonAsync<List<NotificationDTO>>($"api/Notification/user/{userId}") ?? new();
        DashboardData.TotalSchedules = schedules.Count;
        DashboardData.CheckedIn = schedules.Count(x => x.CheckInStatus == "OnTime" || x.CheckInStatus == "Late");
        DashboardData.Late = schedules.Count(x => x.CheckInStatus == "Late");
        DashboardData.Absent = schedules.Count(x => x.CheckInStatus == "Absent");
        //DashboardData.SalaryThisMonth = salary.Where(s => s.Month.Month == DateTime.Now.Month && s.Month.Year == DateTime.Now.Year).Sum(s => s.Amount);
        //DashboardData.Notifications = notifications.Take(5).ToList();
    }

    public class DashboardDataDTO
    {
        public int TotalSchedules { get; set; }
        public int CheckedIn { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public decimal SalaryThisMonth { get; set; }
        public List<NotificationDTO> Notifications { get; set; } = new();
    }
} 