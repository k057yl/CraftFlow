using CraftFlow.Api.Core.Enums;

namespace CraftFlow.Api.Core.Infrastructure.Notifications;

public record NotificationJob(
    NotificationType Type,
    string Recipient,
    string Subject,
    string Body
);