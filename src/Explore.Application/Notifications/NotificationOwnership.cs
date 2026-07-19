// ABOUTME: Defines which system owns a notification decision before delivery is considered.
// ABOUTME: Separates lifecycle ownership from SMTP, RabbitMQ, and other delivery transports.

namespace Explore.Application.Notifications;

public enum NotificationOwnership
{
    AccountAuthority = 2,
    ExternalWorkflowProvider = 3,
    Disabled = 4
}
