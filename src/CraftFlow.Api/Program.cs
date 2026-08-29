using CraftFlow.Api;
using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Analytics;
using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.MRP;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.Api.Modules.Production;
using CraftFlow.Api.Modules.Sales;
using CraftFlow.Api.Modules.Traceability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureAndServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Эндпоинты модулей
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapProductionEndpoints();
app.MapSalesEndpoints();
app.MapAnalyticsEndpoints();
app.MapIdentityEndpoints();
app.MapProcurementEndpoints();
app.MapAgingEndpoints();
app.MapTraceabilityEndpoints();
app.MapMrpEndpoints();

app.Run();