using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.BackgroundWorkers;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.Api.Core.Infrastructure.Converters;
using CraftFlow.Api.Core.Infrastructure.GlobalException;
using CraftFlow.Api.Core.Infrastructure.Identity;
using CraftFlow.Api.Core.Infrastructure.Notifications;
using CraftFlow.Api.Core.Infrastructure.Notifications.Email;
using CraftFlow.Api.Core.Infrastructure.Notifications.TG;
using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.BackgroundWorkers;
using CraftFlow.Api.Modules.CoreDomainServices.Traceability.TraceabilityRead;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

namespace CraftFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAndServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new TrimStringJsonConverter());
            });

        services
            .AddTenantServices()
            .AddSecurityAndCaching(configuration)
            .AddDatabaseStorage(configuration)
            .AddJwtAuthentication(configuration)
            .AddMediatorAndValidation()
            .AddTelegramNotifications()
            .AddBackgroundWorkers();

        services.AddHttpClient<IEmailService, EmailService>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    private static IServiceCollection AddTenantServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    private static IServiceCollection AddSecurityAndCaching(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddMemoryCache();
        return services;
    }

    private static IServiceCollection AddDatabaseStorage(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(AuthConstants.DB_CONNECTION_STRING_PATH);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            }));

        services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<AppDbContext>().Database.GetDbConnection());

        services.AddScoped<TraceabilityReadService>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_PATH]
                        ?? configuration[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_ENV]
                        ?? AuthConstants.DEFAULT_JWT_SECRET;

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        services.AddScoped<ITokenService, TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddMediatorAndValidation(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PermissionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ModuleAccessBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(SubscriptionQuotaBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        return services;
    }

    private static IServiceCollection AddTelegramNotifications(this IServiceCollection services)
    {
        services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>();
        return services;
    }

    private static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
        services.AddHostedService<NotificationBackgroundWorker>();

        services.AddHostedService<LowStockMonitorWorker>();
        services.AddHostedService<ProductionTimerWorker>();
        services.AddHostedService<SubscriptionExpirationWorker>();
        services.AddHostedService<IdentityCleanupWorker>();
        return services;
    }
}