using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using BusinessObject.DTOs;
using System.Text.Json;

namespace WebRazor.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? accessToken = null);
        Task<T?> PostAsync<T>(string endpoint, object data, string? accessToken = null);
        Task<T?> PutAsync<T>(string endpoint, object data, string? accessToken = null);
        Task<bool> DeleteAsync(string endpoint, string? accessToken = null);
        Task<HttpResponseMessage> PostRawAsync(string endpoint, object data, string? accessToken = null);
        Task<HttpResponseMessage> GetRawAsync(string endpoint, string? accessToken = null);
    }

    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                return await client.GetFromJsonAsync<T>(endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Get Error: {ex.Message}");
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                var response = await client.PostAsJsonAsync(endpoint, data);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Post Error: {ex.Message}");
                return default;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                var response = await client.PutAsJsonAsync(endpoint, data);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Put Error: {ex.Message}");
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                var response = await client.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Delete Error: {ex.Message}");
                return false;
            }
        }

        public async Task<HttpResponseMessage> PostRawAsync(string endpoint, object data, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await client.PostAsJsonAsync(endpoint, data);
        }

        public async Task<HttpResponseMessage> GetRawAsync(string endpoint, string? accessToken = null)
        {
            var client = _httpClientFactory.CreateClient("api");
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await client.GetAsync(endpoint);
        }
    }
} 