using CraftFlow.SharedKernel.Enums.Inventory;

namespace CraftFlow.SharedKernel.Dtos.Catalog.Inventory;

public record WarehouseDto(Guid Id, string Name, string? Address, WarehouseType Type);