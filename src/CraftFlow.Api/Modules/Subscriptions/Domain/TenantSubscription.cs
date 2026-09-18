using CraftFlow.SharedKernel.Constants;
using Stateless;

namespace CraftFlow.Api.Modules.Subscriptions.Domain;

public class TenantSubscription
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionState State { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    public SubscriptionPlan Plan { get; private set; } = null!;
    public ICollection<TenantAccessKey> Keys { get; private set; } = new List<TenantAccessKey>();

    private readonly StateMachine<SubscriptionState, SubscriptionTrigger> _machine;

    private TenantSubscription()
    {
        _machine = new StateMachine<SubscriptionState, SubscriptionTrigger>(
            () => State,
            s => State = s
        );

        ConfigureStateMachine();
    }

    public static TenantSubscription CreateFree(Guid tenantId, Guid planId)
    {
        return new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            State = SubscriptionState.Active,
            StartedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = null
        };
    }

    private void ConfigureStateMachine()
    {
        _machine.Configure(SubscriptionState.Active)
            .PermitReentry(SubscriptionTrigger.Activate)
            .Permit(SubscriptionTrigger.Suspend, SubscriptionState.Suspended)
            .Permit(SubscriptionTrigger.Expire, SubscriptionState.Expired)
            .Permit(SubscriptionTrigger.Cancel, SubscriptionState.Cancelled);

        _machine.Configure(SubscriptionState.Suspended)
            .Permit(SubscriptionTrigger.Activate, SubscriptionState.Active)
            .Permit(SubscriptionTrigger.Cancel, SubscriptionState.Cancelled);

        _machine.Configure(SubscriptionState.Expired)
            .Permit(SubscriptionTrigger.Renew, SubscriptionState.Active)
            .Permit(SubscriptionTrigger.Cancel, SubscriptionState.Cancelled);

        _machine.Configure(SubscriptionState.Cancelled)
            .Ignore(SubscriptionTrigger.Cancel);
    }

    public void Activate(DateTime expiresAtUtc)
    {
        EnsureCanFire(SubscriptionTrigger.Activate);
        _machine.Fire(SubscriptionTrigger.Activate);
        ExpiresAtUtc = expiresAtUtc;
    }

    public void ChangePlan(Guid newPlanId)
    {
        PlanId = newPlanId;
    }

    public void Extend(DateTime newExpirationDateUtc)
    {
        ExpiresAtUtc = newExpirationDateUtc;

        if (State == SubscriptionState.Expired && _machine.CanFire(SubscriptionTrigger.Renew))
        {
            _machine.Fire(SubscriptionTrigger.Renew);
        }
    }

    public void Expire()
    {
        EnsureCanFire(SubscriptionTrigger.Expire);
        _machine.Fire(SubscriptionTrigger.Expire);
    }

    public void Cancel()
    {
        EnsureCanFire(SubscriptionTrigger.Cancel);
        _machine.Fire(SubscriptionTrigger.Cancel);
    }

    private void EnsureCanFire(SubscriptionTrigger trigger)
    {
        if (!_machine.CanFire(trigger))
        {
            throw new InvalidOperationException(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED);
        }
    }
}