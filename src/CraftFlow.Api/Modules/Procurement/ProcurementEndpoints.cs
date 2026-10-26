using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Procurement.PurchaseOrders;
using CraftFlow.Api.Modules.Procurement.Suppliers.CreateSupplier;
using CraftFlow.Api.Modules.Procurement.Suppliers.ToggleSupplierActive;
using CraftFlow.Api.Modules.Procurement.Suppliers.UpdateSupplier;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.SharedKernel.Enums.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CraftFlow.Api.Modules.Procurement;

public static class ProcurementEndpoints
{
    public static void MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Procurement")
            .RequireAuthorization();

        // --- SUPPLIERS ---
        group.MapPost(ProcurementConstants.SUPPLIERS, async (CreateSupplierRequest request, ISender sender) =>
        {
            var command = new CreateSupplierCommand(
                request.Name,
                request.LegalName,
                request.TaxId,
                request.Phone,
                request.Email,
                request.Address,
                request.Notes);

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(ProcurementConstants.SUPPLIERS, async (bool? includeInactive, AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var query = dbContext.Suppliers.AsNoTracking();

            if (includeInactive != true)
            {
                query = query.Where(s => s.IsActive);
            }

            var suppliers = await query
                .OrderBy(s => s.Name)
                .Select(s => new SupplierDto(
                    s.Id,
                    s.Name,
                    s.LegalName,
                    s.TaxId,
                    s.Phone,
                    s.Email,
                    s.Address,
                    s.Notes,
                    s.IsActive))
                .ToListAsync(cancellationToken);

            return Results.Ok(suppliers);
        });

        group.MapPut($"{ProcurementConstants.SUPPLIERS}/{{id:guid}}", async (
            Guid id,
            UpdateSupplierRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct) =>
        {
            var userRole = GetUserRoleFromClaims(user);
            var command = new UpdateSupplierCommand(
                id,
                request.Name,
                request.LegalName,
                request.TaxId,
                request.Phone,
                request.Email,
                request.Address,
                request.Notes,
                userRole);

            var result = await sender.Send(command, ct);

            if (result.IsFailure)
            {
                return result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error);
            }

            return Results.Ok();
        });

        group.MapPut($"{ProcurementConstants.SUPPLIERS}/{{id:guid}}/toggle-active", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct) =>
        {
            var userRole = GetUserRoleFromClaims(user);
            var command = new ToggleSupplierActiveCommand(id, userRole);

            var result = await sender.Send(command, ct);

            if (result.IsFailure)
            {
                return result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error);
            }

            return Results.Ok();
        });

        // --- RECEIVE GOODS / STOCK LOTS ---
        group.MapPost(ProcurementConstants.PURCHASE_ORDERS_RECEIVE, async (ReceiveGoodsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }

    private static TenantRole GetUserRoleFromClaims(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value
                        ?? user.FindFirst("role")?.Value
                        ?? user.FindFirst(AuthConstants.Claims.ROLE_SHORT)?.Value;

        if (Enum.TryParse<TenantRole>(roleClaim, ignoreCase: true, out var parsedRole))
        {
            return parsedRole;
        }

        if (int.TryParse(roleClaim, out var roleInt))
        {
            return (TenantRole)roleInt;
        }

        return TenantRole.NONE;
    }
}