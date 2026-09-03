using CraftFlow.Api.BackgroundWorkers;
using CraftFlow.Api.Common.BackgroundWorkers;
using CraftFlow.Api.Common.Behaviors;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.GetActiveAgingLots;
using CraftFlow.Api.Modules.Aging.GetAgingLotDetails;
using CraftFlow.Api.Modules.Production.GetActiveBatchesSummary;
using CraftFlow.Api.Modules.Traceability.TraceabilityRead;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Text;

namespace CraftFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAndServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddTenantServices()
            .AddDatabaseStorage(configuration)
            .AddJwtAuthentication(configuration)
            .AddMediatorAndValidation()
            .AddTelegramNotifications()
            .AddBackgroundWorkers();

        return services;
    }

    private static IServiceCollection AddTenantServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();
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

        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));

        services.AddScoped<TraceabilityReadService>();

        services.AddScoped<GetActiveAgingLotsQueryHandler>();
        services.AddScoped<GetAgingLotDetailsQueryHandler>();
        services.AddScoped<GetActiveBatchesSummaryQueryHandler>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = configuration[AuthConstants.JWT_SECRET_CONFIG_PATH] ?? AuthConstants.DEFAULT_JWT_SECRET;
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

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
                    ValidateAudience = false
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
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(SubscriptionQuotaBehavior<,>));
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
        services.AddHostedService<LowStockMonitorWorker>();
        services.AddHostedService<ProductionTimerWorker>();
        return services;
    }
}