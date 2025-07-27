using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

public class AttendanceAdminIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<AttendanceRecordDTO> AttendanceRecords { get; set; } = new();

    public AttendanceAdminIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        AttendanceRecords = await client.GetFromJsonAsync<List<AttendanceRecordDTO>>("api/Attendance") ?? new();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        var client = _httpClientFactory.CreateClient("api");
        var record = await client.GetFromJsonAsync<AttendanceRecordDTO>($"api/Attendance/{id}");
        if (record == null)
            return NotFound();
        if ((DateTime.UtcNow - record.Date).TotalDays > 2)
        {
            ModelState.AddModelError("", "Cannot update attendance status after 2 days.");
            return Page();
        }
        var content = JsonContent.Create(new { Status = status });
        await client.PutAsync($"api/Attendance/{id}/status", content);
        return RedirectToPage();
    }
} 