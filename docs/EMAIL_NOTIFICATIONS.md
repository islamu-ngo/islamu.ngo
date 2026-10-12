<!-- ABOUTME: Documents current direct SMTP dispatch and account-authority lifecycle email boundaries. -->
<!-- ABOUTME: Separates durable email delivery from the in-app inbox and unimplemented public workflows. -->

# Email And Notifications

> **Audience:** First-party operators | Product owners | Contributors
> **Status:** Active
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Domain/EmailDispatchOutbox.cs`, `src/Explore.Application/Features/ManagedProviderProvisioning/Handlers/Commands/EnsureManagedProviderClientProvisionedCommandHandler.cs`, `src/Explore.Infrastructure/EmailDispatchDrainService.cs`, `src/Explore.Infrastructure/Mail/SmtpEmailService.cs`, `src/Explore.Application/Services/DefaultAccountAuthorityLifecycleEmailService.cs`, `src/Explore.Infrastructure/Services/Keycloak/KeycloakAccountAuthorityLifecycleEmailService.cs`, `src/Explore.API/Controllers/EmailDispatchAdminController.cs`, `src/Explore.API/Controllers/InstanceSettingsController.cs`, `src/Explore.API/HealthChecks/SmtpHealthCheck.cs`, `src/Explore.AppHost/AppHost.cs`, `docker-compose.yml`

## Current Boundary

The repository defines two email mechanisms with different owners:

1. Direct application email has a checked-in producer that persists an `EmailDispatchOutbox` row
   and drains it to SMTP.
2. Identity-lifecycle email has a registered account-authority service and Keycloak adapter, but
   no checked-in application caller currently invokes that service.

These mechanisms are separate from the durable in-app `Notification` inbox. Persisting an inbox row,
changing an actor subscription, or enabling a preference does not enqueue email.

## Capability Matrix

| Capability | Current state |
|---|---|
| Tenant-aware SMTP configuration and send | Implemented with MailKit |
| SMTP connection test and readiness check | Implemented |
| Durable direct-email attempts, receipts, retry, dead-letter, parking, and replay | Implemented |
| Tenant pause/resume and sanitized operator status API | Implemented |
| Optional RabbitMQ pointer transport | Implemented; PostgreSQL remains authoritative |
| Managed-tenant administrator invitation email | Implemented and currently the only checked-in direct-email producer |
| Account-authority verification, password-reset, and email-update service/adapter | Implemented and registered; no checked-in application caller |
| In-app notification inbox and preferences | Implemented separately; see [Notifications](NOTIFICATIONS.md) |
| Automatic email from inbox rows, actor subscriptions, or content publication | Not implemented |
| Waitlist, newsletter, contact-form, digest, or general marketing email | Not implemented |

Do not infer a product workflow from an adapter, table, controller, or background worker alone.
An end-to-end email claim requires a checked-in application producer.

## Direct Application Email

`EmailDispatchOutbox` is the durable source of truth before SMTP handoff. Its current enum contains
only `TenantAdministratorInvitation`. Managed-provider provisioning creates that row when an
administrator invitation was requested, snapshotting the tenant, recipient, subject, bodies,
source operation, user, and correlation identifier.

The delivery flow is:

~~~text
managed provisioning transaction creates EmailDispatchOutbox
  -> TickerQ (default) or hosted-service fallback invokes EmailDispatchDrainService
  -> drain claims the row and restores tenant context
  -> legacy category preference and notification matrix email preference are resolved
  -> disabled ProductAnnouncements delivery is durably marked Skipped
  -> SmtpEmailService sends through MailKit
  -> PostgreSQL records the attempt, receipt, and terminal or retry state
~~~

The drain also supports per-tenant pause, processing-lease recovery, bounded retries,
dead-lettering, unknown-outcome review, operator parking, and replay. The email adapter applies a
Polly resilience pipeline to transient SMTP failures.

When a recipient and mapped category are present, the drain can add signed one-click unsubscribe
headers and body links. The current administrator-invitation kind maps to
`ProductAnnouncements`; both its legacy category preference and its matrix email channel are
checked before provider handoff.

### Optional RabbitMQ Mode

RabbitMQ carries a pointer to the already-persisted email row; it does not carry the recipient,
subject, or body as the delivery ledger. The consumer resolves the PostgreSQL row, delegates to
the same drain, and acknowledges the broker message only after durable outcome handling.

Basic dispatch does not require RabbitMQ. Broker health is reported separately so disabled
RabbitMQ does not make the PostgreSQL/TickerQ path unhealthy.

## Account-Authority Lifecycle Email

Credential-token email does not belong in `EmailDispatchOutbox`. The registered
account-authority lifecycle service exposes requests for:

- email verification;
- password reset; and
- email-update verification.

When called, notification orchestration first records a bounded local intent and, when configured,
a delegation row containing safe correlation and provider references rather than message bodies
or credentials. The Keycloak adapter then requests the corresponding required-action email.
Keycloak owns credential-token generation, template execution, and delivery.

No checked-in application handler or controller currently calls
`IAccountAuthorityLifecycleEmailService`. Therefore the service, adapter, and audit model are
implemented infrastructure, not proof of a live website verification, reset, or email-update
workflow.

Product-lifecycle, platform-operations, and marketing notification ownership remains disabled in
the current routing options. The existence of the lifecycle abstraction must not be described as
a general transactional-email service.

## SMTP Configuration

`SmtpConfigResolver` reads cascading tenant-aware settings and caches resolved configuration per
tenant for five minutes. It returns no configuration when required values such as host or sender
address are missing.

| Setting | Purpose |
|---|---|
| `email.smtp_host` | SMTP host |
| `email.smtp_port` | SMTP port |
| `email.smtp_security` | `None`, `StartTls`, `SslOnConnect`, or `Auto` |
| `email.from_address` | Default sender address |
| `email.from_name` | Default sender display name |
| `email.smtp_timeout_seconds` | Connection and send timeout |
| `email.smtp_skip_cert_validation` | Controlled-development certificate bypass only |
| `email.smtp_username` | Sensitive SMTP username |
| `email.smtp_password` | Sensitive SMTP password |

The raw SMTP message model supports `To`, `Cc`, `Bcc`, `ReplyTo`, custom headers, HTML and plain-
text bodies, attachments, and inline images. `SmtpEmailService` creates a new MailKit client per
send because the client is not thread-safe.

### Secret Handling

The username and password definitions are marked sensitive. Treat them as secrets in logs,
exports, support bundles, screenshots, fixtures, and issue reports. Follow [Secrets](SECRETS.md)
for provider and redaction rules and [Configuration](CONFIGURATION.md) for persisted-setting
boundaries. Do not claim a stronger storage guarantee than those source-backed documents provide.

## Administration And Health

Instance administrators manage SMTP in
`src/Explore.Blazor.Client/Pages/Admin/Instance/Components/InstanceSmtpSection.razor`. The
authenticated instance-settings API reads, updates, and tests the active SMTP configuration at
`/api/instance/settings/smtp` and `/api/instance/settings/smtp/test`.

The authenticated email-dispatch operator API exposes sanitized status plus tenant pause/resume
and row park/replay actions under `/api/admin/email-dispatch`. Status responses omit recipient,
subject, bodies, and raw provider errors. Clients must render write actions from HAL affordances,
not local role guesses.

Readiness reports two distinct concerns:

- `smtp` tests configured SMTP connectivity; missing configuration is degraded and connection
  failure is unhealthy.
- `email-dispatch` reports scheduler configuration, due backlog, retry rows, stale processing
  leases, and dead-letter counts without exposing message content.

## Local Mail Capture

Aspire and Compose start Mailpit for development:

| Runtime | SMTP endpoint | Browser UI |
|---|---|---|
| Aspire host | `localhost:11025` | `http://localhost:18025` |
| Compose service network | `mailpit:1025` | Host port `18025` |

Development seeding reads `MAIL_SMTP_*`, then `SMTP_*` aliases, and otherwise uses Mailpit defaults
when no usable SMTP host is configured. These are local conveniences, not production defaults.

## In-App Notification Boundary

The notification inbox supports authenticated list, detail, unread count, read, archive, snooze,
delete, refresh hints, and preference operations. Actor subscriptions persist preference state.
Current source has no producer that turns actor subscriptions or content publication into inbox
rows, and no producer that turns inbox rows into direct email or Web Push work.

See [Notifications](NOTIFICATIONS.md) for the complete inbox, preference, Web Push, and ownership
boundary. See [Outbox pattern](OUTBOX_PATTERN.md) for the distinction between generic outbox work
and specialized delivery state.

## Unsupported Claims To Avoid

- Automatic actor-subscription or content-publication email fanout.
- Automatic email delivery from an in-app notification.
- Email digests or transport receipts attached to inbox rows.
- A public waitlist, newsletter, contact form, or marketing workflow.
- RabbitMQ as the durable email source of truth.
- Direct application ownership of account-authority credential tokens.
- A live account-lifecycle email workflow based only on the registered service and adapter.

## Troubleshooting

| Symptom | First checks |
|---|---|
| SMTP readiness is degraded or unhealthy | Verify host, port, security mode, credentials, sender address, and network reachability |
| Connection test fails | Run the instance SMTP test and inspect sanitized `SmtpEmailService.TestConnectionAsync` logs |
| A direct-email row is skipped | Check the legacy category preference and matrix `ProductAnnouncements` email choice for its user |
| A row remains pending | Check tenant pause state, scheduler mode, due time, and processing health |
| Retry or dead-letter counts grow | Inspect sanitized operator status; replay only when the HAL action is present and the side effect is safe |
| Local email is absent | Verify the runtime endpoint and inspect Mailpit at `http://localhost:18025` |
| RabbitMQ health is unhealthy | Confirm broker mode is intentional, then inspect connectivity, topology, dead-letter, and parking state |

## Related Documentation

- [Notifications](NOTIFICATIONS.md)
- [Outbox pattern](OUTBOX_PATTERN.md)
- [Configuration](CONFIGURATION.md)
- [Secrets](SECRETS.md)
- [Admin guide](ADMIN_GUIDE.md)
- [Operations](OPERATIONS.md)
- [Troubleshooting](TROUBLESHOOTING.md)
