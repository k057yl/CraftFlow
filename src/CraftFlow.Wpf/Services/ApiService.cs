using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using CraftFlow.Wpf.Models;
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
            BaseAddress = new Uri(ApiConstants.API_BASE_URL)
        };
        _client.DefaultRequestHeaders.Add(ApiConstants.TENANT_HEADER_KEY, ApiConstants.DEFAULT_TENANT_ID);
    }

    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ApiConstants.BEARER_SCHEME, token);
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

    // --- Procurement ---
    public Task<(bool IsSuccess, string ContentOrError)> CreateSupplierAsync(CreateSupplierRequest request) =>
        PostAndReadAsync(Endpoints.SUPPLIERS, request);

    public Task<(bool IsSuccess, string ContentOrError)> ReceiveGoodsAsync(ReceiveGoodsRequest request) =>
        PostAndReadAsync(Endpoints.PURCHASE_ORDERS_RECEIVE, request);

    // --- Aging ---
    public Task<(bool IsSuccess, string ContentOrError)> TransferToAgingAsync(TransferToAgingRequest request) =>
        PostAndReadAsync(Endpoints.AGING_LOTS_TRANSFER, request);

    public Task<(bool IsSuccess, string ContentOrError)> ReleaseFromAgingAsync(ReleaseFromAgingRequest request) =>
        PostAndReadAsync(Endpoints.AGING_LOTS_RELEASE, request);

    // --- MRP ---
    public async Task<Result<MrpReportDto>> GetMrpRequirementsAsync()
    {
        var report = await GetAsync<MrpReportDto>(Endpoints.MRP_REQUIREMENTS);
        if (report != null)
        {
            return Result.Success(report);
        }
        return Result.Failure<MrpReportDto>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
    }

    // --- Generic Delete ---
    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool IsSuccess, string ContentOrError)> DeleteAndReadAsync(string endpoint)
    {
        try
        {
            var response = await _client.DeleteAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            var cleanContent = content.Trim('"').Trim();

            if (response.IsSuccessStatusCode)
            {
                return (true, cleanContent);
            }

            return (false, string.IsNullOrWhiteSpace(cleanContent) ? response.StatusCode.ToString() : cleanContent);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}