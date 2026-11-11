using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPendingInvoices;

public record GetPendingInvoicesQuery : IRequest<Result<List<PendingInvoiceDto>>>;