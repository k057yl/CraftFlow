using System.Text;
using CraftFlow.Api.Common.Behaviors;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CraftFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAndServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. HttpContextAccessor для извлечения TenantId из заголовков/JWT
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // 2. EF Core + PostgreSQL
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            }));

        // 3. JWT Bearer Auth
        var secretKey = configuration["Jwt:SecretKey"] ?? "SUPER_SECRET_KEY_CRAFT_FLOW_2026_OLD_SCHULL_MUST_BE_LONG_ENOUGH";
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

        // 4. MediatR + Pipeline Behaviors (Валидация и Транзакции)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        // 5. FluentValidation (авто-регистрация всех валидаторов из сборок)
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return services;
    }
}