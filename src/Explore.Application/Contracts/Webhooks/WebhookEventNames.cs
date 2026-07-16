// ABOUTME: Central constants for the canonical outgoing webhook event catalog.
// ABOUTME: Prevents string drift between payload builders, tests, providers, and future APIs.

namespace Explore.Application.Contracts.Webhooks;

public static class WebhookEventNames
{
    public const string OrganizationVerified = "organization.verified";
    public const string WebhookTest = "webhook.test";
}
