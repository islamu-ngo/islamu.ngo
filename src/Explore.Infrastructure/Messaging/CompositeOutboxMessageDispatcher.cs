// ABOUTME: Routes durable outbox messages to local application side-effect dispatchers.
// ABOUTME: Handles managed tenant provisioning after durable transaction commits.

using Explore.Application.Contracts.Infrastructure;
using Explore.Application.Features.Management.Handlers;
using Explore.Application.Features.Management.Requests;
using Explore.Domain;
using MediatR;

namespace Explore.Infrastructure.Messaging;

public sealed class CompositeOutboxMessageDispatcher(
    IMediator mediator) : IOutboxMessageDispatcher
{
    public async Task DispatchAsync(OutboxMessage message, CancellationToken ct = default)
    {
        switch (message.EventType)
        {
            case ManagedTenantProvisioningOutboxEvents.ProcessRequested:
                await mediator.Send(
                    new ProcessManagedTenantProvisioningOperationCommand(message.AggregateId, message.Id),
                    ct);
                return;

            default:
                throw new InvalidOperationException(
                    $"Unknown outbox EventType '{message.EventType}' for message {message.Id}. " +
                    $"Add a route in {nameof(CompositeOutboxMessageDispatcher)}.");
        }
    }

    public async Task ReconcileDeadLetterAsync(
        OutboxMessage message,
        CancellationToken ct = default)
    {
        if (message.EventType != ManagedTenantProvisioningOutboxEvents.ProcessRequested)
        {
            return;
        }

        await mediator.Send(
            new ReconcileManagedTenantProvisioningDeadLetterCommand(message.AggregateId, message.Id),
            ct);
    }
}
