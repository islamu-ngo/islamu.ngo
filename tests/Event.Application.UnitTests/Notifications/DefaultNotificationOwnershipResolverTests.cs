// ABOUTME: Unit tests for category-based notification ownership routing.
// ABOUTME: Locks account-authority, disabled local categories, and external delegation defaults.

using Explore.Application.Notifications;
using Microsoft.Extensions.Options;

namespace Event.Application.UnitTests.Notifications;

public sealed class DefaultNotificationOwnershipResolverTests
{
    [Test]
    public async Task ResolveAsync_RoutesIdentityLifecycleToConfiguredAccountAuthority()
    {
        var resolver = CreateResolver(new NotificationRoutingOptions
        {
            DefaultAccountAuthorityKind = AccountAuthorityKind.IslamuOperatedPds
        });

        var decision = await resolver.ResolveAsync(new NotificationIntentDraft(NotificationCategory.IdentityLifecycle));

        await Assert.That(decision.Ownership).IsEqualTo(NotificationOwnership.AccountAuthority);
        await Assert.That(decision.AccountAuthorityKind).IsEqualTo(AccountAuthorityKind.IslamuOperatedPds);
        await Assert.That(decision.RequiresLocalAudit).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_DisablesCategoriesWithoutASupportedOwner()
    {
        var resolver = CreateResolver();

        foreach (var category in new[]
        {
            NotificationCategory.ProductLifecycle,
            NotificationCategory.PlatformOperations,
            NotificationCategory.Marketing
        })
        {
            var decision = await resolver.ResolveAsync(new NotificationIntentDraft(category));

            await Assert.That(decision.Ownership).IsEqualTo(NotificationOwnership.Disabled);
            await Assert.That(decision.RequiresLocalAudit).IsFalse();
            await Assert.That(decision.AccountAuthorityKind).IsEqualTo(AccountAuthorityKind.None);
        }
    }

    [Test]
    public async Task ResolveAsync_RoutesProviderInternalToExternalWorkflowProvider()
    {
        var resolver = CreateResolver(new NotificationRoutingOptions
        {
            ProviderInternalProvider = ExternalWorkflowProviderKind.Other
        });

        var decision = await resolver.ResolveAsync(new NotificationIntentDraft(
            NotificationCategory.ProviderInternal,
            IsUserFacing: false));

        await Assert.That(decision.Ownership).IsEqualTo(NotificationOwnership.ExternalWorkflowProvider);
        await Assert.That(decision.ExternalWorkflowProviderKind).IsEqualTo(ExternalWorkflowProviderKind.Other);
        await Assert.That(decision.RequiresLocalAudit).IsFalse();
    }

    [Test]
    public async Task Constructor_RejectsExternalOwnershipForUnsupportedLocalCategory()
    {
        var options = new NotificationRoutingOptions
        {
            ProductLifecycleOwner = NotificationOwnership.ExternalWorkflowProvider
        };

        await Assert.That(() => CreateResolver(options)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ResolveAsync_HonorsCancellationBeforeReturningDecision()
    {
        var resolver = CreateResolver();
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.That(async () => await resolver.ResolveAsync(
            new NotificationIntentDraft(NotificationCategory.ProductLifecycle),
            source.Token)).Throws<OperationCanceledException>();
    }

    private static DefaultNotificationOwnershipResolver CreateResolver(
        NotificationRoutingOptions? options = null)
    {
        return new DefaultNotificationOwnershipResolver(Options.Create(options ?? new NotificationRoutingOptions()));
    }
}
