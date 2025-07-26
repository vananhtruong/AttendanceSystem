using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

public class DashboardAdminModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public int TotalEmployees { get; set; }
    public int TotalSchedules { get; set; }
    public int PendingRequests { get; set; }
    public decimal TotalSalaryThisMonth { get; set; }

    public DashboardAdminModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        TotalEmployees = await client.GetFromJsonAsync<int>("api/Employee/count");
        TotalSchedules = await client.GetFromJsonAsync<int>("api/WorkSchedule/count");
        //PendingRequests = await client.GetFromJsonAsync<int>("api/CorrectionRequest/pending/count");
        //TotalSalaryThisMonth = await client.GetFromJsonAsync<decimal>("api/Salary/total-this-month");
    }
} 