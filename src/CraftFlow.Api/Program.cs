using CraftFlow.Api;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Analytics;
using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.MRP;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.Api.Modules.Production;
using CraftFlow.Api.Modules.Sales;
using CraftFlow.Api.Modules.Subscriptions;
using CraftFlow.Api.Modules.Traceability;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructureAndServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<TenantAccessKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

await DatabaseInitializer.SeedSaasPlansAsync(app.Services, app.Configuration);

// Эндпоинты модулей
app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapProductionEndpoints();
app.MapSalesEndpoints();
app.MapAnalyticsEndpoints();
app.MapProcurementEndpoints();
app.MapAgingEndpoints();
app.MapTraceabilityEndpoints();
app.MapMrpEndpoints();
app.MapSubscriptionsEndpoints();

app.Run();