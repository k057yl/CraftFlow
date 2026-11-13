using CraftFlow.Api;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Analytics;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog;
using CraftFlow.Api.Modules.CoreDomainServices.Identity;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory;
using CraftFlow.Api.Modules.CoreDomainServices.MRP;
using CraftFlow.Api.Modules.CoreDomainServices.Procurement;
using CraftFlow.Api.Modules.CoreDomainServices.Production;
using CraftFlow.Api.Modules.CoreDomainServices.Sales;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions;
using CraftFlow.Api.Modules.CoreDomainServices.Traceability;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructureAndServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DatabaseInitializer.SeedSaasPlansAsync(services, app.Configuration);
    await SystemAdminSeeder.SeedBigBossAsync(services);
}

// Эндпоинты модулей
app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapProductionEndpoints();
app.MapCheeseMakingEndpoints();
app.MapSalesEndpoints();
app.MapAnalyticsEndpoints();
app.MapProcurementEndpoints();
app.MapAgingEndpoints();
app.MapTraceabilityEndpoints();
app.MapMrpEndpoints();
app.MapSubscriptionsEndpoints();
app.MapSanitationEndpoints();
app.MapFermentationEndpoints();

app.Run();