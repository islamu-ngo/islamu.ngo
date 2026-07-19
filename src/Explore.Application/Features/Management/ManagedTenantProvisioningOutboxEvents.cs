// ABOUTME: Defines the durable outbox event names used by managed tenant provisioning.
// ABOUTME: Keeps dispatch routing constants outside CQRS handler namespaces.

namespace Explore.Application.Features.Management;

public static class ManagedTenantProvisioningOutboxEvents
{
    public const string ProcessRequested = "ManagedTenantProvisioningProcessRequested";
}
