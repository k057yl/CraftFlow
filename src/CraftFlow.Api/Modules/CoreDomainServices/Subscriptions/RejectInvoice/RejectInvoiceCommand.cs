using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RejectInvoice;

public record RejectInvoiceCommand(Guid InvoiceId) : IRequest<Result<bool>>;