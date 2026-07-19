// ABOUTME: Unit tests for composite outbox dispatch routing to retained internal handlers.
// ABOUTME: Verifies managed tenant provisioning dispatch and dead-letter reconciliation.

using Explore.Application.Features.Management.Requests.Commands;
using Explore.Application.Features.Management;
using Explore.Domain;
using Explore.Infrastructure.Messaging;
using MediatR;
using NSubstitute;

namespace Explore.Infrastructure.Tests.Infrastructure;

public sealed class CompositeOutboxMessageDispatcherTests
{
    [Test]
    public async Task DispatchAsync_WithManagedTenantPointer_RoutesProcessCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var dispatcher = new CompositeOutboxMessageDispatcher(mediator);
        Guid operationId = Guid.CreateVersion7();
        Guid messageId = Guid.CreateVersion7();

        await dispatcher.DispatchAsync(new OutboxMessage
        {
            Id = messageId,
            AggregateType = nameof(ManagedTenantProvisioningOperation),
            AggregateId = operationId,
            EventType = ManagedTenantProvisioningOutboxEvents.ProcessRequested
        });

        await mediator.Received(1).Send(
            Arg.Is<ProcessManagedTenantProvisioningOperationCommand>(command =>
                command.OperationId == operationId && command.OutboxMessageId == messageId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DispatchAsync_WithUnknownEventType_Throws()
    {
        var dispatcher = new CompositeOutboxMessageDispatcher(Substitute.For<IMediator>());

        await Assert.That(async () => await dispatcher.DispatchAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateType = "Unknown",
            AggregateId = Guid.NewGuid(),
            EventType = "UnknownEvent",
            Payload = "{}"
        })).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ReconcileDeadLetterAsync_WithManagedTenantPointer_RoutesTerminalCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var dispatcher = new CompositeOutboxMessageDispatcher(mediator);
        Guid operationId = Guid.CreateVersion7();
        Guid messageId = Guid.CreateVersion7();

        await dispatcher.ReconcileDeadLetterAsync(new OutboxMessage
        {
            Id = messageId,
            AggregateType = nameof(ManagedTenantProvisioningOperation),
            AggregateId = operationId,
            EventType = ManagedTenantProvisioningOutboxEvents.ProcessRequested
        });

        await mediator.Received(1).Send(
            Arg.Is<ReconcileManagedTenantProvisioningDeadLetterCommand>(command =>
                command.OperationId == operationId && command.OutboxMessageId == messageId),
            Arg.Any<CancellationToken>());
    }
}
