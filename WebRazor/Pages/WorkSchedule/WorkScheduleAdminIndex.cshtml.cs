using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebRazor.Pages.WorkSchedule
{
    public class WorkScheduleAdminIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<WorkScheduleDTO> WorkSchedules { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SelectedWeek { get; set; }
        public string SelectedWeekString => SelectedWeek ?? DateTime.Today.ToString("yyyy-'W'ww");
        public string WeekRangeString { get; set; }
        public DateTime WeekStart { get; set; }

        public WorkScheduleAdminIndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var allSchedules = await client.GetFromJsonAsync<List<WorkScheduleDTO>>("api/WorkSchedule") ?? new();

            WeekStart = GetWeekStart(SelectedWeek);
            var weekEnd = WeekStart.AddDays(6);
            WorkSchedules = allSchedules.Where(x => x.WorkDate.Date >= WeekStart && x.WorkDate.Date <= weekEnd).ToList();
            WeekRangeString = $"{WeekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";
        }

        private DateTime GetWeekStart(string selectedWeek)
        {
            if (string.IsNullOrEmpty(selectedWeek))
                return DateTime.Today.StartOfWeek(DayOfWeek.Monday);

            var parts = selectedWeek.Split("-W");
            if (parts.Length != 2) return DateTime.Today.StartOfWeek(DayOfWeek.Monday);
            int year = int.Parse(parts[0]);
            int week = int.Parse(parts[1]);
            var jan4 = new DateTime(year, 1, 4);
            int daysOffset = DayOfWeek.Monday - jan4.DayOfWeek;
            var firstMonday = jan4.AddDays(daysOffset);
            return firstMonday.AddDays((week - 1) * 7);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
