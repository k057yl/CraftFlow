using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using CraftFlow.Api.Common.Constants;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using CraftFlow.Wpf.Models;

namespace CraftFlow.Wpf.Services;

public class ApiService
{
    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private readonly HttpClient _client;
    public string? JwtToken { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    private ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(CoreConstants.ApiServiceConstants.API_BASE_URL)
        };
    }

    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            CoreConstants.ApiServiceConstants.BEARER_SCHEME,
            token
        );
    }

    public void ClearAuthToken()
    {
        JwtToken = null;
        _client.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _client.GetAsync(endpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HandleUnauthorized();
                return default;
            }

            if (!response.IsSuccessStatusCode) return default;

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch
        {
            return default;
        }
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var response = await _client.PostAsJsonAsync(endpoint, data);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HandleUnauthorized();
        }
        return response;
    }

    public async Task<(bool IsSuccess, string ContentOrError)> PostAndReadAsync<T>(string endpoint, T data)
    {
        try
        {
            var response = await _client.PostAsJsonAsync(endpoint, data);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HandleUnauthorized();
                return (false, "UI_ERROR_UNAUTHORIZED");
            }

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

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _client.DeleteAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HandleUnauthorized();
                return false;
            }
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HandleUnauthorized();
                return (false, "UI_ERROR_UNAUTHORIZED");
            }

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

    private void HandleUnauthorized()
    {
        ClearAuthToken();

        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToAuth();
            }
        });
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
}