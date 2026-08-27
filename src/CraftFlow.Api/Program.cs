using CraftFlow.Api;
using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureAndServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Эндпоинты модулей
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();

app.Run();