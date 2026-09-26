using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "production");

            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "saas");

            migrationBuilder.RenameTable(
                name: "warehouses",
                schema: "public",
                newName: "warehouses",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "public",
                newName: "users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "units_of_measure",
                schema: "public",
                newName: "units_of_measure",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "tenant_subscriptions",
                schema: "public",
                newName: "tenant_subscriptions",
                newSchema: "saas");

            migrationBuilder.RenameTable(
                name: "tenant_access_keys",
                schema: "public",
                newName: "tenant_access_keys",
                newSchema: "saas");

            migrationBuilder.RenameTable(
                name: "subscription_plans",
                schema: "public",
                newName: "subscription_plans",
                newSchema: "saas");

            migrationBuilder.RenameTable(
                name: "subscription_payments",
                schema: "public",
                newName: "subscription_payments",
                newSchema: "saas");

            migrationBuilder.RenameTable(
                name: "storage_locations",
                schema: "public",
                newName: "storage_locations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_lots",
                schema: "public",
                newName: "stock_lots",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "sales_orders",
                schema: "public",
                newName: "sales_orders",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "sales_order_items",
                schema: "public",
                newName: "sales_order_items",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "recipes",
                schema: "public",
                newName: "recipes",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "recipe_ingredients",
                schema: "public",
                newName: "recipe_ingredients",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "raw_materials",
                schema: "public",
                newName: "raw_materials",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "products",
                schema: "public",
                newName: "products",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "production_batches",
                schema: "public",
                newName: "production_batches",
                newSchema: "production");

            migrationBuilder.RenameTable(
                name: "organizations",
                schema: "public",
                newName: "organizations",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "customers",
                schema: "public",
                newName: "customers",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "consumed_ingredients",
                schema: "public",
                newName: "consumed_ingredients",
                newSchema: "production");

            migrationBuilder.RenameIndex(
                name: "IX_purchase_order_items_PurchaseOrderId",
                schema: "procurement",
                table: "purchase_order_items",
                newName: "IX_PURCHASE_ORDER_ITEMS_ORDER");

            migrationBuilder.RenameIndex(
                name: "IX_aging_lot_items_AgingLotId",
                schema: "aging",
                table: "aging_lot_items",
                newName: "IX_AGING_LOT_ITEMS_LOT");

            migrationBuilder.RenameIndex(
                name: "IX_tenant_access_keys_TenantId",
                schema: "saas",
                table: "tenant_access_keys",
                newName: "IX_ACCESS_KEYS_TENANT");

            migrationBuilder.RenameIndex(
                name: "IX_tenant_access_keys_KeyHash",
                schema: "saas",
                table: "tenant_access_keys",
                newName: "IX_ACCESS_KEYS_HASH");

            migrationBuilder.RenameIndex(
                name: "IX_subscription_plans_Code",
                schema: "saas",
                table: "subscription_plans",
                newName: "IX_SUBSCRIPTION_PLANS_CODE");

            migrationBuilder.RenameIndex(
                name: "IX_sales_order_items_SalesOrderId",
                schema: "sales",
                table: "sales_order_items",
                newName: "IX_SALES_ORDER_ITEMS_ORDER");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "saas",
                table: "tenant_subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "KeyPrefix",
                schema: "saas",
                table: "tenant_access_keys",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "saas",
                table: "tenant_access_keys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "saas",
                table: "subscription_payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                schema: "sales",
                table: "sales_orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                schema: "sales",
                table: "sales_order_items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "sales",
                table: "sales_order_items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "sales",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLIERS_TENANT_NAME",
                schema: "procurement",
                table: "suppliers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_STATUS",
                schema: "procurement",
                table: "purchase_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_SUPPLIER",
                schema: "procurement",
                table: "purchase_orders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_BATCH",
                schema: "aging",
                table: "aging_lots",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_CHAMBER",
                schema: "aging",
                table: "aging_lots",
                column: "AgingChamberId");

            migrationBuilder.CreateIndex(
                name: "IX_AGING_CHAMBERS_TENANT_NAME",
                schema: "aging",
                table: "aging_chambers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_OTP_EXPIRATION",
                schema: "identity",
                table: "users",
                column: "OtpExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_LOTS_LOOKUP",
                schema: "inventory",
                table: "stock_lots",
                columns: new[] { "TenantId", "WarehouseId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_CUSTOMER",
                schema: "sales",
                table: "sales_orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_STATUS",
                schema: "sales",
                table: "sales_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations",
                columns: new[] { "IsActive", "IsSelfDeactivated", "DeactivatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CUSTOMERS_TENANT_NAME",
                schema: "sales",
                table: "customers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CONSUMED_INGREDIENTS_BATCH",
                schema: "production",
                table: "consumed_ingredients",
                column: "ProductionBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SUPPLIERS_TENANT_NAME",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "IX_PURCHASE_ORDERS_STATUS",
                schema: "procurement",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_PURCHASE_ORDERS_SUPPLIER",
                schema: "procurement",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_AGING_LOTS_BATCH",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.DropIndex(
                name: "IX_AGING_LOTS_CHAMBER",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.DropIndex(
                name: "IX_AGING_CHAMBERS_TENANT_NAME",
                schema: "aging",
                table: "aging_chambers");

            migrationBuilder.DropIndex(
                name: "IX_USERS_OTP_EXPIRATION",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_STOCK_LOTS_LOOKUP",
                schema: "inventory",
                table: "stock_lots");

            migrationBuilder.DropIndex(
                name: "IX_SALES_ORDERS_CUSTOMER",
                schema: "sales",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_SALES_ORDERS_STATUS",
                schema: "sales",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_CUSTOMERS_TENANT_NAME",
                schema: "sales",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_CONSUMED_INGREDIENTS_BATCH",
                schema: "production",
                table: "consumed_ingredients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "saas",
                table: "tenant_subscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "saas",
                table: "tenant_access_keys");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "saas",
                table: "subscription_payments");

            migrationBuilder.RenameTable(
                name: "warehouses",
                schema: "inventory",
                newName: "warehouses",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "units_of_measure",
                schema: "catalog",
                newName: "units_of_measure",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "tenant_subscriptions",
                schema: "saas",
                newName: "tenant_subscriptions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "tenant_access_keys",
                schema: "saas",
                newName: "tenant_access_keys",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "subscription_plans",
                schema: "saas",
                newName: "subscription_plans",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "subscription_payments",
                schema: "saas",
                newName: "subscription_payments",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "storage_locations",
                schema: "inventory",
                newName: "storage_locations",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "stock_lots",
                schema: "inventory",
                newName: "stock_lots",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "sales_orders",
                schema: "sales",
                newName: "sales_orders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "sales_order_items",
                schema: "sales",
                newName: "sales_order_items",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "recipes",
                schema: "catalog",
                newName: "recipes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "recipe_ingredients",
                schema: "catalog",
                newName: "recipe_ingredients",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "raw_materials",
                schema: "catalog",
                newName: "raw_materials",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "products",
                schema: "catalog",
                newName: "products",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "production_batches",
                schema: "production",
                newName: "production_batches",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "organizations",
                schema: "identity",
                newName: "organizations",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "customers",
                schema: "sales",
                newName: "customers",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "consumed_ingredients",
                schema: "production",
                newName: "consumed_ingredients",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_PURCHASE_ORDER_ITEMS_ORDER",
                schema: "procurement",
                table: "purchase_order_items",
                newName: "IX_purchase_order_items_PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_AGING_LOT_ITEMS_LOT",
                schema: "aging",
                table: "aging_lot_items",
                newName: "IX_aging_lot_items_AgingLotId");

            migrationBuilder.RenameIndex(
                name: "IX_ACCESS_KEYS_TENANT",
                schema: "public",
                table: "tenant_access_keys",
                newName: "IX_tenant_access_keys_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ACCESS_KEYS_HASH",
                schema: "public",
                table: "tenant_access_keys",
                newName: "IX_tenant_access_keys_KeyHash");

            migrationBuilder.RenameIndex(
                name: "IX_SUBSCRIPTION_PLANS_CODE",
                schema: "public",
                table: "subscription_plans",
                newName: "IX_subscription_plans_Code");

            migrationBuilder.RenameIndex(
                name: "IX_SALES_ORDER_ITEMS_ORDER",
                schema: "public",
                table: "sales_order_items",
                newName: "IX_sales_order_items_SalesOrderId");

            migrationBuilder.AlterColumn<string>(
                name: "KeyPrefix",
                schema: "public",
                table: "tenant_access_keys",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                schema: "public",
                table: "sales_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                schema: "public",
                table: "sales_order_items",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "public",
                table: "sales_order_items",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "public",
                table: "customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
