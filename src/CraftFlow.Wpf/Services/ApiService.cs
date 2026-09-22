using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.MRP;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Auth;
using CraftFlow.SharedKernel.Result;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Auth;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

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
    public Guid? CurrentTenantId { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    public event Action? OnAuthStateChanged;

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

    public async Task<bool> ValidateAndRefreshCurrentUserAsync()
    {
        if (!IsAuthenticated) return false;

        if (CurrentUser == null)
        {
            ClearAuthToken();
            return await Task.FromResult(false);
        }

        if (CurrentTenantId.HasValue && CurrentTenantId.Value != Guid.Empty)
        {
            SetTenantHeader(CurrentTenantId.Value);
        }

        OnAuthStateChanged?.Invoke();

        return await Task.FromResult(true);
    }

    public void SetAuthToken(string token, bool rememberMe = true)
    {
        JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            CoreConstants.ApiServiceConstants.BEARER_SCHEME,
            token
        );

        CurrentUser = ParseUserFromJwt(token);
        SetTenantHeader(CurrentTenantId);

        if (rememberMe)
        {
            SaveTokenToDisk(token);
        }
        else
        {
            DeleteTokenFromDisk();
        }

        OnAuthStateChanged?.Invoke();
    }

    public void SetTenantHeader(Guid? tenantId)
    {
        CurrentTenantId = tenantId;
        if (_client.DefaultRequestHeaders.Contains(CoreConstants.MultiTenancy.HEADER_TENANT_ID))
        {
            _client.DefaultRequestHeaders.Remove(CoreConstants.MultiTenancy.HEADER_TENANT_ID);
        }

        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            _client.DefaultRequestHeaders.Add(CoreConstants.MultiTenancy.HEADER_TENANT_ID, tenantId.Value.ToString());
        }
    }

    public void ClearTenantHeader()
    {
        SetTenantHeader(null);
    }

    public void ClearAuthToken()
    {
        JwtToken = null;
        CurrentUser = null;
        _client.DefaultRequestHeaders.Authorization = null;
        ClearTenantHeader();
        DeleteTokenFromDisk();

        OnAuthStateChanged?.Invoke();
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
        catch { }
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
        catch { }
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
        catch { }
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

            if (root.TryGetProperty(AuthConstants.Claims.EMAIL, out var emailProp) ||
                root.TryGetProperty("email", out emailProp) ||
                root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", out emailProp) ||
                root.TryGetProperty("sub", out emailProp))
            {
                user.Email = emailProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty(AuthConstants.Claims.FULL_NAME, out var nameProp) ||
                root.TryGetProperty("name", out nameProp) ||
                root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out nameProp))
            {
                user.FullName = nameProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty(CoreConstants.MultiTenancy.CLAIM_TENANT_ID, out var tenantProp) &&
                Guid.TryParse(tenantProp.GetString(), out var tenantId))
            {
                CurrentTenantId = tenantId;
            }

            user.Role = ExtractRoleFromJwt(root);

            return user;
        }
        catch
        {
            return null;
        }
    }

    private static TenantRole ExtractRoleFromJwt(JsonElement root)
    {
        string[] roleClaimNames =
        {
            "role",
            "Role",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            AuthConstants.Claims.ROLE_SHORT
        };

        foreach (var claimName in roleClaimNames)
        {
            if (!root.TryGetProperty(claimName, out var roleProp)) continue;

            if (roleProp.ValueKind == JsonValueKind.Number && roleProp.TryGetInt32(out int roleInt))
            {
                return (TenantRole)roleInt;
            }

            if (roleProp.ValueKind == JsonValueKind.String)
            {
                var roleStr = roleProp.GetString();
                if (int.TryParse(roleStr, out int parsedInt))
                {
                    return (TenantRole)parsedInt;
                }

                if (Enum.TryParse<TenantRole>(roleStr, ignoreCase: true, out var parsedRole))
                {
                    return parsedRole;
                }
            }
        }

        return TenantRole.None;
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

    // --- Identity ---
    public async Task<List<TenantUserDto>> GetTenantUsersAsync()
    {
        var users = await GetAsync<List<TenantUserDto>>(IdentityConstants.USERS);
        return users ?? new List<TenantUserDto>();
    }

    public Task<(bool IsSuccess, string ContentOrError)> ToggleUserStatusAsync(Guid userId) =>
        PostAndReadAsync($"{IdentityConstants.USERS}/{userId}/toggle-status", new { });

    // --- Procurement ---
    public Task<(bool IsSuccess, string ContentOrError)> CreateSupplierAsync(CreateSupplierRequest request) =>
        PostAndReadAsync(ProcurementConstants.SUPPLIERS, request);

    public Task<(bool IsSuccess, string ContentOrError)> ReceiveGoodsAsync(ReceiveGoodsRequest request) =>
        PostAndReadAsync(ProcurementConstants.PURCHASE_ORDERS_RECEIVE, request);

    // --- Aging ---
    public Task<(bool IsSuccess, string ContentOrError)> TransferToAgingAsync(TransferToAgingRequest request) =>
        PostAndReadAsync(AgingConstants.AGING_LOTS_TRANSFER, request);

    public Task<(bool IsSuccess, string ContentOrError)> ReleaseFromAgingAsync(ReleaseFromAgingRequest request) =>
        PostAndReadAsync(AgingConstants.AGING_LOTS_RELEASE, request);

    // --- MRP ---
    public async Task<Result<MrpReportDto>> GetMrpRequirementsAsync()
    {
        var report = await GetAsync<MrpReportDto>(MrpConstants.MRP_REQUIREMENTS);
        if (report != null)
        {
            return Result.Success(report);
        }
        return Result.Failure<MrpReportDto>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
    }
}