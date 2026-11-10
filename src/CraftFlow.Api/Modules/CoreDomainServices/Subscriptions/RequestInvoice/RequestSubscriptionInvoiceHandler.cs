using CraftFlow.Api.Core.Infrastructure.Email;
using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
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

        var adminEmail = "karakatsiyacraft@gmail.com";
        var emailSubject = $"[CraftFlow Payment] Заявка на счет: {orgName} ({request.PlanCode})";

        var emailHtml = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa;'>
                <div style='max-width: 550px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #1c7ed6; margin-top: 0;'>Новая заявка на модуль</h2>
                    <p><b>Организация:</b> {orgName}</p>
                    <p><b>Tenant ID:</b> <code>{tenantId}</code></p>
                    <p><b>Email:</b> {userEmail}</p>
                    <p><b>Модуль:</b> <b style='color: #2b8a3e;'>{request.PlanCode}</b></p>
                    <p><b>Период:</b> {request.Days} дней</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #868e96;'>После оплаты зайди в AdminPage приложения и подними тариф этой сыроварне.</p>
                </div>
            </div>";

        var isSent = await _emailService.SendEmailAsync(adminEmail, emailSubject, emailHtml);

        if (!isSent)
        {
            return Result.Failure(Error.Failure("EMAIL_SEND_FAILED"));
        }

        return Result.Success();
    }
}