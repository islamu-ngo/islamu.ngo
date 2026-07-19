// ABOUTME: Unit tests for the webhook event catalog query handler.
// ABOUTME: Verifies provider-neutral catalog mapping includes schema, examples, retention, and fields.

using System.Text.Json;
using Explore.Application.Contracts.Persistence;
using Explore.Application.Contracts.Webhooks;
using Explore.Application.Features.Webhooks.Handlers.Queries;
using Explore.Application.Features.Webhooks.Requests.Queries;
using Explore.Application.Webhooks;
using Explore.Domain;
using NSubstitute;

namespace Event.Application.UnitTests.Features.Webhooks.Queries;

public sealed class GetWebhookEventTypesQueryHandlerTests
{
    [Test]
    public async Task Handle_ReturnsCanonicalCatalogWithSchemaExamplesAndFields()
    {
        var persistedId = Guid.CreateVersion7();
        var eventTypeRepository = Substitute.For<IWebhookEventTypeRepository>();
        eventTypeRepository.GetByNamesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(
                [
                    new WebhookEventType
                    {
                        Id = persistedId,
                        Name = WebhookEventNames.OrganizationVerified,
                        GroupName = "organization",
                        Description = "Raised when an organization is verified.",
                        SchemaJson = """{"type":"object"}""",
                        SchemaVersion = 1,
                        IsPublic = true,
                        IsEnabled = true,
                        PayloadRetentionDays = 14
                    }
                ]);
        var handler = new GetWebhookEventTypesQueryHandler(
            new WebhookEventTypeRegistry(),
            new WebhookEventSchemaProvider(),
            eventTypeRepository);

        var result = await handler.Handle(new GetWebhookEventTypesQuery(), CancellationToken.None);

        await Assert.That(result).Count().IsEqualTo(2);
        await Assert.That(result.Select(eventType => eventType.Name)).Contains(WebhookEventNames.OrganizationVerified);
        await Assert.That(result.Select(eventType => eventType.Name)).Contains(WebhookEventNames.WebhookTest);

        var verified = result.Single(eventType => eventType.Name == WebhookEventNames.OrganizationVerified);
        await Assert.That(verified.Id).IsEqualTo(persistedId);
        await Assert.That(verified.GroupName).IsEqualTo("organization");
        await Assert.That(verified.SchemaVersion).IsEqualTo(1);
        await Assert.That(verified.IsPublic).IsTrue();
        await Assert.That(verified.IsEnabled).IsTrue();
        await Assert.That(verified.PayloadRetentionDays).IsEqualTo(14);
        await Assert.That(verified.DataFields.Select(field => field.Name)).Contains("organizationId");

        using var schema = JsonDocument.Parse(verified.SchemaJson);
        using var example = JsonDocument.Parse(verified.ExamplePayloadJson);
        await Assert.That(schema.RootElement.GetProperty("type").GetString()).IsEqualTo("object");
        await Assert.That(example.RootElement.GetProperty("type").GetString()).IsEqualTo(WebhookEventNames.OrganizationVerified);
    }
}
