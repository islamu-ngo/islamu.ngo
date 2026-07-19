// ABOUTME: Controlled notification categories used by ownership routing decisions.
// ABOUTME: Prevents provider and delivery settings from replacing product-domain responsibility rules.

namespace Explore.Application.Notifications;

public enum NotificationCategory
{
    IdentityLifecycle = 1,
    ProductLifecycle = 2,
    ProviderInternal = 7,
    PlatformOperations = 8,
    Marketing = 9
}
