using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cheese_making");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.EnsureSchema(
                name: "fermentation");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "procurement");

            migrationBuilder.EnsureSchema(
                name: "sanitation");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "saas");

            migrationBuilder.CreateTable(
                name: "aging_chambers",
                schema: "cheese_making",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetHumidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aging_chambers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aging_lots",
                schema: "cheese_making",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgingChamberId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InitialQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aging_lots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    ChangesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cheese_batches",
                schema: "cheese_making",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedOutputQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ActualOutputQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OverheadPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BrewingCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsTelegramNotified = table.Column<bool>(type: "boolean", nullable: false),
                    DiscardReason = table.Column<string>(type: "text", nullable: true),
                    UnitsCount = table.Column<int>(type: "integer", nullable: false),
                    HeadWeightsSerialized = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cheese_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cheese_consumed_ingredients",
                schema: "cheese_making",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheeseBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cheese_consumed_ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fermentation_batches",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubstrateWeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SaltPercentage = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    InitialPh = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    CurrentPh = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    TargetTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    VesselInfo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActualOutputQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitsCount = table.Column<int>(type: "integer", nullable: false),
                    OverheadPercentage = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    DiscardReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Stage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovedToColdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiscardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRetentionNoticeSentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sales_orders",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sanitation_logs",
                schema: "sanitation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ChemicalsUsed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sanitation_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sanitation_rules",
                schema: "sanitation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: true),
                    MaxBatchCount = table.Column<int>(type: "integer", nullable: true),
                    NextScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequiredChemicals = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sanitation_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sanitation_tasks",
                schema: "sanitation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChemicalsUsed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sanitation_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_invoices",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysRequested = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payments",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysAdded = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetModule = table.Column<int>(type: "integer", nullable: false),
                    MaxMonthlyBatches = table.Column<int>(type: "integer", nullable: false),
                    MaxWarehouses = table.Column<int>(type: "integer", nullable: false),
                    MaxChambers = table.Column<int>(type: "integer", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_admins",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "units_of_measure",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    BaseUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversionFactor = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units_of_measure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_units_of_measure_units_of_measure_BaseUnitId",
                        column: x => x.BaseUnitId,
                        principalSchema: "catalog",
                        principalTable: "units_of_measure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OtpCodeHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OtpExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouses",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aging_lot_items",
                schema: "cheese_making",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgingLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InitialWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    DiscardReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aging_lot_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aging_lot_items_aging_lots_AgingLotId",
                        column: x => x.AgingLotId,
                        principalSchema: "cheese_making",
                        principalTable: "aging_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fermentation_consumed_ingredients",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FermentationBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_consumed_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fermentation_consumed_ingredients_fermentation_batches_Ferm~",
                        column: x => x.FermentationBatchId,
                        principalSchema: "fermentation",
                        principalTable: "fermentation_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fermentation_readings",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FermentationBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ph = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DegassedOrBurped = table.Column<bool>(type: "boolean", nullable: false),
                    BrineToppedUp = table.Column<bool>(type: "boolean", nullable: false),
                    SensoryNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fermentation_readings_fermentation_batches_FermentationBatc~",
                        column: x => x.FermentationBatchId,
                        principalSchema: "fermentation",
                        principalTable: "fermentation_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_items",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_purchase_orders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "procurement",
                        principalTable: "purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_order_items",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_order_items_sales_orders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "sales",
                        principalTable: "sales_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscriptions",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_subscriptions_subscription_plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "saas",
                        principalTable: "subscription_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.UniqueConstraint("AK_products_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_products_units_of_measure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "catalog",
                        principalTable: "units_of_measure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "raw_materials",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_raw_materials_units_of_measure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "catalog",
                        principalTable: "units_of_measure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_lots",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitsCount = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: true),
                    ProductionBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_lots_units_of_measure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "catalog",
                        principalTable: "units_of_measure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_members",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    JoinedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_members_organizations_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "identity",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_members_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_locations",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChamberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CurrentVolume = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentProductionBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchesProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    WashCycleBatchInterval = table.Column<int>(type: "integer", nullable: false),
                    LastWashedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextWashDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storage_locations_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "inventory",
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "CHEESE_MAKING"),
                    TargetOutputQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsAgingRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultMinAgingDays = table.Column<int>(type: "integer", nullable: true),
                    TargetDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipes_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipes_products_TenantId_ProductId",
                        columns: x => new { x.TenantId, x.ProductId },
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_lot_items",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsSold = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lot_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_lot_items_stock_lots_StockLotId",
                        column: x => x.StockLotId,
                        principalSchema: "inventory",
                        principalTable: "stock_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_lot_storage_locations",
                schema: "inventory",
                columns: table => new
                {
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lot_storage_locations", x => new { x.StockLotId, x.StorageLocationId });
                    table.ForeignKey(
                        name: "FK_stock_lot_storage_locations_stock_lots_StockLotId",
                        column: x => x.StockLotId,
                        principalSchema: "inventory",
                        principalTable: "stock_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalSchema: "catalog",
                        principalTable: "raw_materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "catalog",
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AGING_CHAMBERS_TENANT_NAME",
                schema: "cheese_making",
                table: "aging_chambers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOT_ITEMS_LOT",
                schema: "cheese_making",
                table: "aging_lot_items",
                column: "AgingLotId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_BATCH",
                schema: "cheese_making",
                table: "aging_lots",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_CHAMBER",
                schema: "cheese_making",
                table: "aging_lots",
                column: "AgingChamberId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_TENANT_STATE",
                schema: "cheese_making",
                table: "aging_lots",
                columns: new[] { "TenantId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOGS_TENANT_CREATED_AT",
                schema: "identity",
                table: "audit_logs",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CHEESE_BATCHES_TIMER_MONITORING",
                schema: "cheese_making",
                table: "cheese_batches",
                columns: new[] { "TenantId", "State", "IsTelegramNotified", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CHEESE_CONSUMED_INGREDIENTS_BATCH",
                schema: "cheese_making",
                table: "cheese_consumed_ingredients",
                column: "CheeseBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CUSTOMERS_TENANT_NAME",
                schema: "sales",
                table: "customers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_BATCHES_TENANT_STAGE",
                schema: "fermentation",
                table: "fermentation_batches",
                columns: new[] { "TenantId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_CONSUMED_INGREDIENTS_BATCH_ID",
                schema: "fermentation",
                table: "fermentation_consumed_ingredients",
                column: "FermentationBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_CONSUMED_INGREDIENTS_TENANT_ID",
                schema: "fermentation",
                table: "fermentation_consumed_ingredients",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_READINGS_BATCH_RECORDED_AT",
                schema: "fermentation",
                table: "fermentation_readings",
                columns: new[] { "FermentationBatchId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "Role", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MEMBERS_TENANT_USER",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_UserId",
                schema: "identity",
                table: "organization_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations",
                columns: new[] { "Status", "DeactivatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_products_TenantId_Id",
                schema: "catalog",
                table: "products",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_UnitOfMeasureId",
                schema: "catalog",
                table: "products",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDER_ITEMS_ORDER",
                schema: "procurement",
                table: "purchase_order_items",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_SUPPLIER",
                schema: "procurement",
                table: "purchase_orders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_TENANT_STATUS",
                schema: "procurement",
                table: "purchase_orders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_raw_materials_UnitOfMeasureId",
                schema: "catalog",
                table: "raw_materials",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RecipeId",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_ProductId",
                schema: "catalog",
                table: "recipes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_TenantId_ProductId",
                schema: "catalog",
                table: "recipes",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_ITEMS_ORDER",
                schema: "sales",
                table: "sales_order_items",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_CUSTOMER",
                schema: "sales",
                table: "sales_orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_TENANT_STATUS",
                schema: "sales",
                table: "sales_orders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SANITATION_LOGS_TENANT_TARGET",
                schema: "sanitation",
                table: "sanitation_logs",
                columns: new[] { "TenantId", "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_SANITATION_LOGS_TENANT_TASK",
                schema: "sanitation",
                table: "sanitation_logs",
                columns: new[] { "TenantId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_SANITATION_RULES_TENANT_TARGET",
                schema: "sanitation",
                table: "sanitation_rules",
                columns: new[] { "TenantId", "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_SANITATION_TASKS_TENANT_STATUS",
                schema: "sanitation",
                table: "sanitation_tasks",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SANITATION_TASKS_TENANT_TARGET",
                schema: "sanitation",
                table: "sanitation_tasks",
                columns: new[] { "TenantId", "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_lot_items_StockLotId",
                schema: "inventory",
                table: "stock_lot_items",
                column: "StockLotId");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_LOTS_LOOKUP",
                schema: "inventory",
                table: "stock_lots",
                columns: new[] { "TenantId", "WarehouseId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_STORAGE_LOCATIONS_TENANT_WAREHOUSE",
                schema: "inventory",
                table: "storage_locations",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_storage_locations_WarehouseId",
                schema: "inventory",
                table: "storage_locations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_Status",
                schema: "saas",
                table: "subscription_invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_TenantId",
                schema: "saas",
                table: "subscription_invoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payments_TenantId",
                schema: "saas",
                table: "subscription_payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SUBSCRIPTION_PLANS_CODE",
                schema: "saas",
                table: "subscription_plans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLIERS_TENANT_NAME",
                schema: "procurement",
                table: "suppliers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLIERS_TENANT_TAX_ID",
                schema: "procurement",
                table: "suppliers",
                columns: new[] { "TenantId", "TaxId" },
                filter: "\"TaxId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SYSTEM_ADMINS_USER_ID",
                schema: "identity",
                table: "system_admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_PlanId",
                schema: "saas",
                table: "tenant_subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_TenantId",
                schema: "saas",
                table: "tenant_subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_TenantId_ExpiresAtUtc",
                schema: "saas",
                table: "tenant_subscriptions",
                columns: new[] { "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_units_of_measure_BaseUnitId",
                schema: "catalog",
                table: "units_of_measure",
                column: "BaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_units_of_measure_TenantId_Code",
                schema: "catalog",
                table: "units_of_measure",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_EMAIL",
                schema: "identity",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_OTP_EXPIRATION",
                schema: "identity",
                table: "users",
                column: "OtpExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aging_chambers",
                schema: "cheese_making");

            migrationBuilder.DropTable(
                name: "aging_lot_items",
                schema: "cheese_making");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "cheese_batches",
                schema: "cheese_making");

            migrationBuilder.DropTable(
                name: "cheese_consumed_ingredients",
                schema: "cheese_making");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "fermentation_consumed_ingredients",
                schema: "fermentation");

            migrationBuilder.DropTable(
                name: "fermentation_readings",
                schema: "fermentation");

            migrationBuilder.DropTable(
                name: "organization_members",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "purchase_order_items",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "recipe_ingredients",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "sales_order_items",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "sanitation_logs",
                schema: "sanitation");

            migrationBuilder.DropTable(
                name: "sanitation_rules",
                schema: "sanitation");

            migrationBuilder.DropTable(
                name: "sanitation_tasks",
                schema: "sanitation");

            migrationBuilder.DropTable(
                name: "stock_lot_items",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_lot_storage_locations",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "storage_locations",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "subscription_invoices",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "subscription_payments",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "system_admins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tenant_subscriptions",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "aging_lots",
                schema: "cheese_making");

            migrationBuilder.DropTable(
                name: "fermentation_batches",
                schema: "fermentation");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "purchase_orders",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "raw_materials",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "recipes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "sales_orders",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "stock_lots",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "warehouses",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "subscription_plans",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "units_of_measure",
                schema: "catalog");
        }
    }
}
