using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Core.Behaviors;

public sealed class PlanFeatureBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ITenantContext _tenantContext;
    private readonly ITenantEntitlementService _entitlementService;

    public PlanFeatureBehavior(
        ITenantContext tenantContext,
        ITenantEntitlementService entitlementService)
    {
        _tenantContext = tenantContext;
        _entitlementService = entitlementService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequirePlanFeature featureRequest)
        {
            return await next();
        }

        if (_tenantContext.IsSuperAdmin || _tenantContext.Role == TenantRole.SUPER_ADMIN)
        {
            return await next();
        }

        if (_tenantContext.TenantId == Guid.Empty)
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Forbidden(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var hasAccess = request is IRequireModule moduleRequest
            ? await _entitlementService.HasModuleFeatureAsync(moduleRequest.RequiredModule, featureRequest.RequiredFeature, cancellationToken)
            : await _entitlementService.HasOrganizationFeatureAsync(featureRequest.RequiredFeature, cancellationToken);

        if (!hasAccess)
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Forbidden(ErrorCodes.Saas.FEATURE_NOT_AVAILABLE));
        }

        return await next();
    }
}