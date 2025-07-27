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

public class CorrectionRequestAdminIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<CorrectionRequestDTO> CorrectionRequests { get; set; } = new();

    public CorrectionRequestAdminIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        CorrectionRequests = await client.GetFromJsonAsync<List<CorrectionRequestDTO>>("api/CorrectionRequest") ?? new();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("api");
        var request = await client.GetFromJsonAsync<CorrectionRequestDTO>($"api/CorrectionRequest/{id}");
        if (request == null)
            return NotFound();
        if ((DateTime.UtcNow - request.Date).TotalDays > 2)
        {
            ModelState.AddModelError("", "Cannot approve after 2 days.");
            return Page();
        }
        await client.PostAsync($"api/CorrectionRequest/{id}/approve", null);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, string rejectReason)
    {
        var client = _httpClientFactory.CreateClient("api");
        var request = await client.GetFromJsonAsync<CorrectionRequestDTO>($"api/CorrectionRequest/{id}");
        if (request == null)
            return NotFound();
        if ((DateTime.UtcNow - request.Date).TotalDays > 2)
        {
            ModelState.AddModelError("", "Cannot reject after 2 days.");
            return Page();
        }
        if (string.IsNullOrWhiteSpace(rejectReason))
        {
            ModelState.AddModelError("", "Reject reason is required.");
            return Page();
        }
        var content = JsonContent.Create(new { rejectReason });
        await client.PostAsync($"api/CorrectionRequest/{id}/reject", content);
        return RedirectToPage();
    }
} 