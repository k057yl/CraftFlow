namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;

public static class PlanUpgradeCalculator
{
    /// <summary>
    /// Рассчитывает новую дату окончания подписки с учетом пропорционального пересчета остатка дней при смене уровня тарифа.
    /// Веса уровней: FREE = 0, STARTER = 1, STANDARD = 2, PRO = 4.
    /// При продлении того же тарифа дни суммируются 1:1.
    /// При переходе на другой тариф остаток дней умножается на коэффициент (вес_старого / вес_нового),
    /// чтобы оплаченный остаток дешевого тарифа не превращался в полный срок дорогого тарифа.
    /// </summary>
    public static DateTime CalculateNewExpirationUtc(
        string? currentPlanCode,
        DateTime? currentExpiresAtUtc,
        string targetPlanCode,
        int daysToAdd,
        DateTime nowUtc)
    {
        double remainingDays = currentExpiresAtUtc.HasValue && currentExpiresAtUtc.Value > nowUtc
            ? (currentExpiresAtUtc.Value - nowUtc).TotalDays
            : 0;

        if (remainingDays <= 0 || string.IsNullOrWhiteSpace(currentPlanCode))
        {
            var directEnd = nowUtc.AddDays(daysToAdd);
            return directEnd < nowUtc ? nowUtc : directEnd;
        }

        int currentWeight = GetTierWeight(currentPlanCode);
        int targetWeight = GetTierWeight(targetPlanCode);

        double convertedRemainingDays;

        if (currentPlanCode.Equals(targetPlanCode, StringComparison.OrdinalIgnoreCase))
        {
            convertedRemainingDays = remainingDays;
        }
        else if (currentWeight == 0 || targetWeight == 0)
        {
            convertedRemainingDays = 0;
        }
        else
        {
            convertedRemainingDays = remainingDays * ((double)currentWeight / targetWeight);
        }

        var resultDate = nowUtc.AddDays(convertedRemainingDays + daysToAdd);
        return resultDate < nowUtc ? nowUtc : resultDate;
    }

    private static int GetTierWeight(string planCode)
    {
        var suffix = planCode.Split('_').LastOrDefault()?.ToUpperInvariant();
        return suffix switch
        {
            "STARTER" => 1,
            "STANDARD" => 2,
            "PRO" => 4,
            _ => 0
        };
    }
}