using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using CraftFlow.Api.Common.Constants;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Auth;

namespace CraftFlow.Wpf.Services;

public class ApiService
{
    private const string TOKEN_FILE_NAME = "auth_token.dat";
    private const string APP_FOLDER_NAME = "CraftFlow";

    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private readonly HttpClient _client;
    private readonly string _tokenFilePath;

    public string? JwtToken { get; private set; }
    public UserProfileDto? CurrentUser { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    private ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(CoreConstants.ApiServiceConstants.API_BASE_URL)
        };

        _tokenFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            APP_FOLDER_NAME,
            TOKEN_FILE_NAME);

        LoadSavedToken();
    }

    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            CoreConstants.ApiServiceConstants.BEARER_SCHEME,
            token
        );

        CurrentUser = ParseUserFromJwt(token);
        SaveTokenToDisk(token);
    }

    public void ClearAuthToken()
    {
        JwtToken = null;
        CurrentUser = null;
        _client.DefaultRequestHeaders.Authorization = null;
        DeleteTokenFromDisk();
    }

    private void SaveTokenToDisk(string token)
    {
        try
        {
            var dir = Path.GetDirectoryName(_tokenFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }
            File.WriteAllText(_tokenFilePath, token);
        }
        catch
        {
        }
    }

    private void DeleteTokenFromDisk()
    {
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
        }
        catch
        {
        }
    }

    private void LoadSavedToken()
    {
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                var savedToken = File.ReadAllText(_tokenFilePath);
                if (!string.IsNullOrWhiteSpace(savedToken))
                {
                    JwtToken = savedToken;
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        CoreConstants.ApiServiceConstants.BEARER_SCHEME,
                        savedToken
                    );
                    CurrentUser = ParseUserFromJwt(savedToken);
                }
            }
        }
        catch
        {
        }
    }

    public void SetSubscriptionKey(string apiKey)
    {
        if (_client.DefaultRequestHeaders.Contains(AuthConstants.Headers.SUBSCRIPTION_KEY))
        {
            _client.DefaultRequestHeaders.Remove(AuthConstants.Headers.SUBSCRIPTION_KEY);
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client.DefaultRequestHeaders.Add(AuthConstants.Headers.SUBSCRIPTION_KEY, apiKey);
        }
    }

    private UserProfileDto? ParseUserFromJwt(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var jsonBytes = Convert.FromBase64String(paddedPayload);

            using var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;

            var user = new UserProfileDto();

            if (root.TryGetProperty(AuthConstants.Claims.EMAIL, out var emailProp))
            {
                user.Email = emailProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty(AuthConstants.Claims.FULL_NAME, out var nameProp))
            {
                user.FullName = nameProp.GetString() ?? string.Empty;
            }

            user.IsAdmin = CheckRoleInJwt(root, AuthConstants.Claims.ROLE_SHORT) ||
                           CheckRoleInJwt(root, AuthConstants.Claims.ROLE_FULL);

            return user;
        }
        catch
        {
            return null;
        }
    }

    private static bool CheckRoleInJwt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var roleProp)) return false;

        if (roleProp.ValueKind == JsonValueKind.String)
        {
            return string.Equals(roleProp.GetString(), AuthConstants.Roles.ADMIN, StringComparison.OrdinalIgnoreCase);
        }

        if (roleProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in roleProp.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.String &&
                    string.Equals(element.GetString(), AuthConstants.Roles.ADMIN, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
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
                return (false, UiConstants.Messages.RELEASE_ERROR);
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
                return (false, UiConstants.Messages.RELEASE_ERROR);
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