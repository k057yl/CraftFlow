using CraftFlow.Api.Common.Behaviors;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAndServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. HttpContextAccessor для извлечения TenantId из заголовков
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // 2. EF Core + PostgreSQL
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            }));

        // 3. MediatR + Pipeline Behaviors (Валидация и Транзакции)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        // 4. FluentValidation (авто-регистрация всех валидаторов из сборок)
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return services;
    }
}