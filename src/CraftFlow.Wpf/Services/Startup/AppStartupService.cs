using CraftFlow.Api.Modules.Analytics;
using CraftFlow.SharedKernel.Dtos.Dashboard;

namespace CraftFlow.Wpf.Services.Startup;

public class AppStartupService
{
    public async Task<StartupResult> RunAsync(IProgress<StartupProgress> progress)
    {
        try
        {
            progress.Report(new StartupProgress(15, "Инициализация компонентов..."));
            await Task.Delay(100);

            progress.Report(new StartupProgress(35, "Проверка авторизации..."));
            if (!ApiService.Instance.IsAuthenticated)
            {
                progress.Report(new StartupProgress(100, "Сессия не найдена. Переход к входу..."));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            progress.Report(new StartupProgress(55, "Валидация сессии..."));
            var isValidSession = await ApiService.Instance.ValidateAndRefreshCurrentUserAsync();

            if (!isValidSession)
            {
                progress.Report(new StartupProgress(100, "Истёк срок сессии..."));
                return new StartupResult { ResultType = StartupResultType.AuthFailed };
            }

            progress.Report(new StartupProgress(80, "Загрузка данных дашборда..."));
            var summary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(AnalyticConstants.DASHBOARD);

            progress.Report(new StartupProgress(100, "Запуск системы..."));

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
}