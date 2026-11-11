using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ApproveInvoice;

public record ApproveInvoiceCommand(Guid InvoiceId) : IRequest<Result<bool>>;