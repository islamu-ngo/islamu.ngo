// ABOUTME: Unit tests for webhook payload envelope creation and minimization.
// ABOUTME: Guards stable hashes, retention calculation, required fields, and strict data allow-lists.

using System.Text;
using System.Text.Json;
using Explore.Application.Contracts.Webhooks;
using Explore.Application.Webhooks;

namespace Event.Application.UnitTests.Webhooks;

public sealed class DefaultWebhookPayloadBuilderTests
{
    private static readonly Guid MessageId = Guid.Parse("018f0000-0000-7000-8000-000000000999");
    private static readonly Guid TenantId = Guid.Parse("018e4e5c-7f00-7000-8000-000000000001");
    private static readonly Guid AggregateId = Guid.Parse("018f0000-0000-7000-8000-000000000001");
    private static readonly DateTimeOffset OccurredAt = new(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);

    private readonly DefaultWebhookPayloadBuilder _builder = new(new WebhookEventTypeRegistry());

    [Test]
    public async Task BuildAsync_ForVerifiedOrganization_CreatesStableEnvelopeHashAndRetention()
    {
        var context = CreateContext(
            WebhookEventNames.OrganizationVerified,
            new Dictionary<string, object?>
            {
                ["organizationId"] = AggregateId.ToString(),
                ["status"] = "Verified",
                ["internalNote"] = "must not be included"
            });

        var result = await _builder.BuildAsync(context, CancellationToken.None);

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.Envelope!.Id).IsEqualTo(MessageId);
        await Assert.That(result.Envelope.Type).IsEqualTo(WebhookEventNames.OrganizationVerified);
        await Assert.That(result.Envelope.Version).IsEqualTo(1);
        await Assert.That(result.PayloadHash).IsNotNull();
        await Assert.That(result.PayloadHash!.Length).IsEqualTo(71);
        await Assert.That(result.PayloadHash).StartsWith("sha256:");
        await Assert.That(result.PayloadRetentionUntil).IsEqualTo(OccurredAt.AddDays(14));
        await Assert.That(PayloadJson(result)).Contains("organizationId");
        await Assert.That(PayloadJson(result)).DoesNotContain("internalNote");

        using var parsed = JsonDocument.Parse(result.PayloadBytes!);
        await Assert.That(parsed.RootElement.GetProperty("tenantId").GetGuid()).IsEqualTo(TenantId);
        await Assert.That(parsed.RootElement.GetProperty("data").GetProperty("organizationId").GetString()).IsEqualTo(AggregateId.ToString());
    }

    [Test]
    public async Task BuildAsync_WhenRequiredFieldMissing_FailsClosed()
    {
        var context = CreateContext(
            WebhookEventNames.OrganizationVerified,
            new Dictionary<string, object?>
            {
                ["organizationId"] = AggregateId.ToString()
            });

        var result = await _builder.BuildAsync(context, CancellationToken.None);

        await Assert.That(result.Succeeded).IsFalse();
        await Assert.That(result.FailureCategory).IsEqualTo("missing_required_payload_field");
        await Assert.That(result.PayloadBytes).IsNull();
    }

    [Test]
    public async Task BuildAsync_WhenEventTypeUnknown_FailsClosed()
    {
        var context = CreateContext(
            "unknown.event",
            new Dictionary<string, object?>
            {
                ["eventId"] = AggregateId.ToString()
            });

        var result = await _builder.BuildAsync(context, CancellationToken.None);

        await Assert.That(result.Succeeded).IsFalse();
        await Assert.That(result.FailureCategory).IsEqualTo("unknown_event_type");
        await Assert.That(result.Envelope).IsNull();
    }

    private static WebhookEventBuildContext CreateContext(
        string eventType,
        IReadOnlyDictionary<string, object?> data) =>
        new(
            MessageId,
            TenantId,
            eventType,
            MessageId.ToString(),
            "Organization",
            AggregateId,
            OccurredAt,
            data);

    private static string PayloadJson(WebhookPayloadBuildResult result) =>
        Encoding.UTF8.GetString(result.PayloadBytes!);
}
