using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CraftFlow.Wpf.Services;
public class ApiService
{
    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private readonly HttpClient _client;
    public string? JwtToken { get; private set; }

    private ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5109/")
        };
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "00000000-0000-0000-0000-000000000001");
    }

    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task<T?> GetAsync<T>(string endpoint) => _client.GetFromJsonAsync<T>(endpoint);

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data) =>
        await _client.PostAsJsonAsync(endpoint, data);
}
