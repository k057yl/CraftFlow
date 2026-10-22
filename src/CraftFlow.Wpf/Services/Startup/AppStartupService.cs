using CraftFlow.Api.Modules.Analytics;
using CraftFlow.SharedKernel.Dtos.Dashboard;

namespace CraftFlow.Wpf.Services.Startup;

public class AppStartupService
{
    private const string MSG_INIT = "LBL_STARTUP_INIT";
    private const string MSG_AUTH_CHECK = "LBL_STARTUP_AUTH_CHECK";
    private const string MSG_AUTH_VALIDATE = "LBL_STARTUP_AUTH_VALIDATE";
    private const string MSG_LOAD_DICTIONARIES = "LBL_STARTUP_LOAD_DICTIONARIES";
    private const string MSG_LOAD_DASHBOARD = "LBL_STARTUP_LOAD_DASHBOARD";
    private const string MSG_COMPLETE = "LBL_STARTUP_COMPLETE";

    private const string ERR_NO_SESSION = "ERR_SESSION_NOT_FOUND";
    private const string ERR_SESSION_EXPIRED = "ERR_SESSION_EXPIRED";
    private const string ERR_DASHBOARD_EMPTY = "ERR_DASHBOARD_DATA_NULL";
    private const string ERR_API_UNAVAILABLE = "ERR_API_UNAVAILABLE";

    private const int PCT_INIT = 10;
    private const int PCT_AUTH_CHECK = 25;
    private const int PCT_AUTH_VALIDATE = 40;
    private const int PCT_DICTIONARIES = 65;
    private const int PCT_DASHBOARD = 90;
    private const int PCT_COMPLETE = 100;

    public async Task<StartupResult> RunAsync(IProgress<StartupProgress> progress)
    {
        try
        {
            progress?.Report(new StartupProgress(PCT_INIT, MSG_INIT));

            var isApiReady = await WaitForApiReadyAsync(maxRetries: 10, delayMs: 500);
            if (!isApiReady)
            {
                return new StartupResult
                {
                    ResultType = StartupResultType.CriticalError,
                    ErrorMessage = ERR_API_UNAVAILABLE
                };
            }

            progress?.Report(new StartupProgress(PCT_AUTH_CHECK, MSG_AUTH_CHECK));
            if (!ApiService.Instance.IsAuthenticated)
            {
                progress?.Report(new StartupProgress(PCT_COMPLETE, ERR_NO_SESSION));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            progress?.Report(new StartupProgress(PCT_AUTH_VALIDATE, MSG_AUTH_VALIDATE));
            var isValidSession = await ApiService.Instance.ValidateAndRefreshCurrentUserAsync();

            if (!isValidSession)
            {
                progress?.Report(new StartupProgress(PCT_COMPLETE, ERR_SESSION_EXPIRED));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            progress?.Report(new StartupProgress(PCT_DICTIONARIES, MSG_LOAD_DICTIONARIES));

            progress?.Report(new StartupProgress(PCT_DASHBOARD, MSG_LOAD_DASHBOARD));
            var summary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(AnalyticConstants.DASHBOARD);

            if (summary == null)
            {
                return new StartupResult
                {
                    ResultType = StartupResultType.CriticalError,
                    ErrorMessage = ERR_DASHBOARD_EMPTY
                };
            }

            progress?.Report(new StartupProgress(PCT_COMPLETE, MSG_COMPLETE));

            return new StartupResult
            {
                ResultType = StartupResultType.Success,
                InitialData = summary
            };
        }
        catch (Exception ex)
        {
            return new StartupResult
            {
                ResultType = StartupResultType.CriticalError,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<bool> WaitForApiReadyAsync(int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var isAlive = await ApiService.Instance.CheckHealthAsync();
                if (isAlive) return true;
            }
            catch
            {
            }

            await Task.Delay(delayMs);
        }

        return false;
    }
}