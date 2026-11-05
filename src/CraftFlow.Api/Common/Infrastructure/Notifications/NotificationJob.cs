namespace CraftFlow.Api.Common.Infrastructure.Notifications;
public record NotificationJob(
    NotificationType Type,
    string Recipient,
    string Subject,
    string Body
);