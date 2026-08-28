using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CraftFlow.Wpf.Services;

public class ApiService
{
    private const string API_BASE_URL = "http://localhost:5109/";
    private const string TENANT_HEADER_KEY = "X-Tenant-Id";
    private const string DEFAULT_TENANT_ID = "00000000-0000-0000-0000-000000000001";
    private const string BEARER_SCHEME = "Bearer";

    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private readonly HttpClient _client;
    public string? JwtToken { get; private set; }

    private ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(API_BASE_URL)
        };
        _client.DefaultRequestHeaders.Add(TENANT_HEADER_KEY, DEFAULT_TENANT_ID);
    }

    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BEARER_SCHEME, token);
    }

    public Task<T?> GetAsync<T>(string endpoint) => _client.GetFromJsonAsync<T>(endpoint);

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data) =>
        await _client.PostAsJsonAsync(endpoint, data);

    public async Task<(bool IsSuccess, string ContentOrError)> PostAndReadAsync<T>(string endpoint, T data)
    {
        var response = await _client.PostAsJsonAsync(endpoint, data);
        var content = await response.Content.ReadAsStringAsync();
        var cleanContent = content.Trim('"').Trim();

        if (response.IsSuccessStatusCode)
        {
            return (true, cleanContent);
        }

        return (false, string.IsNullOrWhiteSpace(cleanContent) ? response.StatusCode.ToString() : cleanContent);
    }
}