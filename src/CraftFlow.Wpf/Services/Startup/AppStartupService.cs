using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Dashboard;

namespace CraftFlow.Wpf.Services.Startup;

public class AppStartupService
{
    private const string MSG_INIT = "LBL_STARTUP_INIT";
    private const string MSG_WAIT_API = "LBL_STARTUP_WAIT_API";
    private const string MSG_AUTH_CHECK = "LBL_STARTUP_AUTH_CHECK";
    private const string MSG_AUTH_VALIDATE = "LBL_STARTUP_AUTH_VALIDATE";
    private const string MSG_LOAD_DICTIONARIES = "LBL_STARTUP_LOAD_DICTIONARIES";
    private const string MSG_LOAD_DASHBOARD = "LBL_STARTUP_LOAD_DASHBOARD";
    private const string MSG_COMPLETE = "LBL_STARTUP_COMPLETE";

    private const string ERR_NO_SESSION = "ERR_SESSION_NOT_FOUND";
    private const string ERR_SESSION_EXPIRED = "ERR_SESSION_EXPIRED";
    private const string ERR_DASHBOARD_EMPTY = "ERR_DASHBOARD_DATA_NULL";
    private const string ERR_API_UNAVAILABLE = "ERR_API_UNAVAILABLE";

    public async Task<StartupResult> RunAsync(IProgress<StartupProgress> progress)
    {
        try
        {
            // --- ЭТАП 1: 0% -> 20% (Ожидание и проверка готовности API) ---
            progress?.Report(new StartupProgress(0, LocalizationService.Get(MSG_INIT)));

            var isApiReady = await WaitForApiReadyAsync(progress, maxRetries: 10, delayMs: 400);
            if (!isApiReady)
            {
                return new StartupResult
                {
                    ResultType = StartupResultType.CriticalError,
                    ErrorMessage = LocalizationService.Get(ERR_API_UNAVAILABLE)
                };
            }
            progress?.Report(new StartupProgress(20, LocalizationService.Get(MSG_WAIT_API)));

            // --- ЭТАП 2: 20% -> 35% (Пинг проверки наличия сессии) ---
            await RunStepWithPingsAsync(progress, 20, 35, MSG_AUTH_CHECK, async () =>
            {
                await Task.Delay(150);
            });

            if (!ApiService.Instance.IsAuthenticated)
            {
                progress?.Report(new StartupProgress(100, LocalizationService.Get(ERR_NO_SESSION)));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            // --- ЭТАП 3: 35% -> 60% (Валидация и обновление JWT токена) ---
            bool isValidSession = false;
            await RunStepWithPingsAsync(progress, 35, 60, MSG_AUTH_VALIDATE, async () =>
            {
                isValidSession = await ApiService.Instance.ValidateAndRefreshCurrentUserAsync();
            });

            if (!isValidSession)
            {
                progress?.Report(new StartupProgress(100, LocalizationService.Get(ERR_SESSION_EXPIRED)));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            // --- ЭТАП 4: 60% -> 75% (Загрузка справочников / конфигурации) ---
            await RunStepWithPingsAsync(progress, 60, 75, MSG_LOAD_DICTIONARIES, async () =>
            {
                await Task.Delay(200);
            });

            // --- ЭТАП 5: 75% -> 95% (Загрузка сводки дашборда) ---
            DashboardSummaryDto? summary = null;
            await RunStepWithPingsAsync(progress, 75, 95, MSG_LOAD_DASHBOARD, async () =>
            {
                summary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(AnalyticConstants.DASHBOARD);
            });

            if (summary == null)
            {
                return new StartupResult
                {
                    ResultType = StartupResultType.CriticalError,
                    ErrorMessage = LocalizationService.Get(ERR_DASHBOARD_EMPTY)
                };
            }

            // --- ФИНАЛ: 100% ---
            progress?.Report(new StartupProgress(100, LocalizationService.Get(MSG_COMPLETE)));

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

    private async Task<bool> WaitForApiReadyAsync(IProgress<StartupProgress> progress, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            // Плавный тикер от 0% до 20% по пыткам подключения
            int stepPercent = (int)((i / (double)maxRetries) * 20);
            progress?.Report(new StartupProgress(stepPercent, LocalizationService.Get(MSG_WAIT_API)));

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

    private static async Task RunStepWithPingsAsync(
        IProgress<StartupProgress> progress,
        int fromPercent,
        int toPercent,
        string messageKey,
        Func<Task> action)
    {
        using var cts = new CancellationTokenSource();
        var localizedMessage = LocalizationService.Get(messageKey);

        var pingTask = Task.Run(async () =>
        {
            int current = fromPercent;
            while (!cts.Token.IsCancellationRequested && current < toPercent - 1)
            {
                progress?.Report(new StartupProgress(current, localizedMessage));
                current++;
                await Task.Delay(40, cts.Token);
            }
        }, cts.Token);

        try
        {
            await action();
        }
        finally
        {
            cts.Cancel();
            try { await pingTask; } catch (OperationCanceledException) { }

            progress?.Report(new StartupProgress(toPercent, localizedMessage));
        }
    }
}