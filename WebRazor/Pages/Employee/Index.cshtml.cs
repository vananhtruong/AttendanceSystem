using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Linq;

public class EmployeeIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<UserDTO> Employees { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string SearchKeyword { get; set; }
    public int PageSize { get; set; } = 10;
    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;
    public int CurrentPage => Page;
    public int TotalPages { get; set; }
    public List<UserDTO> PagedEmployees { get; set; } = new();

    public EmployeeIndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var accessToken = HttpContext.Session.GetString("AccessToken");
        var client = _httpClientFactory.CreateClient("api");
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (!string.IsNullOrEmpty(SearchKeyword))
        {
            Employees = await client.GetFromJsonAsync<List<UserDTO>>($"api/Employee/search?keyword={SearchKeyword}") ?? new();
        }
        else
        {
            Employees = await client.GetFromJsonAsync<List<UserDTO>>("api/Employee") ?? new();
        }
        // Pagination logic
        int total = Employees.Count;
        TotalPages = (int)System.Math.Ceiling(total / (double)PageSize);
        PagedEmployees = Employees.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
    }

    public async Task<IActionResult> OnPostBanAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("api");
        await client.PutAsync($"api/Employee/ban/{id}", null);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnbanAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("api");
        await client.PutAsync($"api/Employee/unban/{id}", null);
        return RedirectToPage();
    }
} 