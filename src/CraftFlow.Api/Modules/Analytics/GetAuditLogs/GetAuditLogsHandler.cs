using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Analytics;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Analytics.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<List<AuditLogDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetAuditLogsQueryHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<List<AuditLogDto>>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(x => x.UserEmail.ToLower().Contains(term) ||
                                     x.Details.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action);
        }

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.FromDate.Value.ToUniversalTime());
        }

        if (request.ToDate.HasValue)
        {
            var endOfDay = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedAtUtc <= endOfDay.ToUniversalTime());
        }

        var logs = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(500)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.Action,
                x.Details,
                x.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return Result<List<AuditLogDto>>.Success(logs);
    }
}