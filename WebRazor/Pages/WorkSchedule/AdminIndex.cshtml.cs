using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

public class WorkScheduleAdminIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<WorkScheduleDTO> WorkSchedules { get; set; } = new();

    public WorkScheduleAdminIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        WorkSchedules = await client.GetFromJsonAsync<List<WorkScheduleDTO>>("api/WorkSchedule") ?? new();
    }
} 