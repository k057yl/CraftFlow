using CraftFlow.Api.Core.Infrastructure.Notifications.Email;
using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RequestInvoice;

public class RequestSubscriptionInvoiceHandler : IRequestHandler<RequestSubscriptionInvoiceCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;

    public RequestSubscriptionInvoiceHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _emailService = emailService;
    }

    public async Task<Result> Handle(RequestSubscriptionInvoiceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == tenantId, cancellationToken);

        var userEmail = _tenantContext.UserEmail;
        var orgName = organization?.Name ?? "Не указана";
        var invoice = SubscriptionInvoice.Create(tenantId, request.PlanCode, request.Days);
        _dbContext.SubscriptionInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var adminEmail = "karakatsiyacraft@gmail.com";
        var emailSubject = $"[CraftFlow Payment] Новая заявка #{invoice.Id.ToString()[..8]}: {orgName} ({request.PlanCode})";

        var emailHtml = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa;'>
                <div style='max-width: 550px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #1c7ed6; margin-top: 0;'>Заявка на оплату #{invoice.Id.ToString()[..8]}</h2>
                    <p><b>Организация:</b> {orgName}</p>
                    <p><b>Tenant ID:</b> <code>{tenantId}</code></p>
                    <p><b>Email клиента:</b> {userEmail}</p>
                    <p><b>Модуль / Тариф:</b> <b style='color: #2b8a3e;'>{request.PlanCode}</b></p>
                    <p><b>Период:</b> {request.Days} дней</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='font-size: 13px; color: #495057;'>Подтвердить оплату можно прямо в AdminPage десктопного приложения.</p>
                </div>
            </div>";

        _ = _emailService.SendEmailAsync(adminEmail, emailSubject, emailHtml);

        return Result.Success();
    }
}