// ABOUTME: Default canonical event catalog for outgoing platform webhooks.
// ABOUTME: Provides stable event descriptors used by payload builders, APIs, providers, and documentation.

using Explore.Application.Contracts.Webhooks;

namespace Explore.Application.Webhooks;

public sealed class WebhookEventTypeRegistry : IWebhookEventTypeRegistry
{
    private static readonly WebhookEventTypeDescriptor[] Descriptors =
    [
        CreateEventDescriptor(
            WebhookEventNames.OrganizationVerified,
            "organization",
            "Raised when an organization is verified.",
            [
                Field("organizationId", "Organization identifier.", "018f0000-0000-7000-8000-000000000201"),
                Field("status", "Organization verification status.", "Verified")
            ]),
        CreateEventDescriptor(
            WebhookEventNames.WebhookTest,
            "webhook",
            "Raised when an administrator schedules a LocalProvider test delivery for one endpoint.",
            [
                Field("endpointId", "Webhook endpoint identifier receiving the test delivery.", "018f0000-0000-7000-8000-000000000301"),
                Field("consumerId", "Webhook consumer that owns the endpoint.", "018f0000-0000-7000-8000-000000000302"),
                Field("providerMode", "Consumer provider mode at the time the test was scheduled.", "Local"),
                Field("requestedAt", "UTC timestamp when the test delivery was requested.", "2026-07-02T10:00:00.0000000+00:00")
            ],
            payloadRetentionDays: 1)
    ];

    private static readonly Dictionary<string, WebhookEventTypeDescriptor> ByName =
        Descriptors.ToDictionary(descriptor => descriptor.Name, StringComparer.Ordinal);

    public IReadOnlyCollection<WebhookEventTypeDescriptor> GetAll() => Descriptors;

    public WebhookEventTypeDescriptor? FindByName(string name) =>
        ByName.GetValueOrDefault(name);

    public bool IsKnownEventType(string name) =>
        ByName.ContainsKey(name);

    public static bool IsValidEventTypeName(string name) =>
        !string.IsNullOrWhiteSpace(name)
        && name.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character is '-' or '_' or '.');

    private static WebhookEventTypeDescriptor CreateEventDescriptor(
        string name,
        string groupName,
        string description,
        IReadOnlyList<WebhookEventDataFieldDescriptor> dataFields,
        int payloadRetentionDays = 14,
        int schemaVersion = 1) =>
        new(
            name,
            groupName,
            description,
            schemaVersion,
            IsPublic: true,
            IsEnabled: true,
            payloadRetentionDays,
            dataFields);

    private static WebhookEventDataFieldDescriptor Field(
        string name,
        string description,
        object? example,
        string jsonType = WebhookJsonSchemaTypes.Text,
        bool required = true) =>
        new(name, jsonType, description, example, required);
}
