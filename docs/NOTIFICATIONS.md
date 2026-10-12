<!-- ABOUTME: Documents current in-app notification, preference, Web Push, and ownership boundaries. -->
<!-- ABOUTME: Separates inbox state from actor subscriptions, account-authority email, and delivery queues. -->

# Notifications

> **Audience:** Contributors | Operators | Integrators | AI agents
> **Status:** Active
> **Owner:** Product/Platform
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Domain/Notification.cs`, `src/Explore.Domain/NotificationIntent.cs`, `src/Explore.Application/Features/Notifications/`, `src/Explore.Application/Notifications/DefaultNotificationOrchestrator.cs`, `src/Explore.Application/Notifications/NotificationRoutingOptions.cs`, `src/Explore.Application/Services/NotificationRefreshStreamService.cs`, `src/Explore.Application/Services/DefaultAccountAuthorityLifecycleEmailService.cs`, `src/Explore.Infrastructure/Services/Keycloak/KeycloakAccountAuthorityLifecycleEmailService.cs`, `src/Explore.API/Controllers/NotificationController.cs`, `src/Explore.API/Controllers/ActorSubscriptionController.cs`, `src/Explore.Blazor.Client/Helpers/NotificationNavigationHelper.cs`, `src/Explore.Blazor.Client/Pages/Notifications/Notifications.razor`, `src/Explore.Infrastructure/WebPush/WebPushDispatchDrainService.cs`

## Current Boundary

The repository has three related but distinct models:

1. `Notification` is a durable, user-owned in-app inbox row.
2. `NotificationIntent` records routing ownership and safe local audit for notification requests.
3. Email and Web Push use dedicated durable delivery state rather than treating an inbox row as a
   transport message.

The inbox API, UI, preference matrix, browser-push enrollment, and delivery infrastructure are
implemented. Current application source does not contain a producer that turns actor
subscriptions or content publication into `Notification` rows, and it does not enqueue Web Push
automatically from an in-app notification. Do not describe those missing connections as live
fanout.

## In-App Inbox

`Notification` rows are authenticated, tenant-scoped, and owned by one user. They retain lookup
metadata for type, scope, reason, and optional Organization/Group/User entity context, together
with read, archive, snooze, audit, and soft-delete state.

| Operation | Current state |
|---|---|
| Paginated list and filters | Implemented |
| Ownership-checked detail | Implemented |
| Unread count | Implemented |
| Mark one or all read | Implemented |
| Archive/unarchive | Implemented |
| Snooze/unsnooze | Implemented |
| Soft delete | Implemented |
| Mark unread | Not implemented |
| Automatic subscription/content fanout | Not implemented |

List filters use the API field names, including `notificationTypeId`, `notificationScopeId`, and
`notificationReasonId`. The notification repository always applies current-user ownership to
inbox reads and mutations.

## API Surface

All inbox endpoints require authentication:

| Action | Endpoint |
|---|---|
| List | `GET /api/notification` |
| Detail | `GET /api/notification/{id}` |
| Unread count | `GET /api/notification/unread-count` |
| Refresh stream | `GET /api/notification/stream` |
| Mark read | `PATCH /api/notification/{id}/read` |
| Mark all read | `POST /api/notification/read-all` |
| Archive/unarchive | `PATCH /api/notification/{id}/archive?archive=true|false` |
| Snooze/unsnooze | `PATCH /api/notification/{id}/snooze?snoozedUntil=...` |
| Soft delete | `DELETE /api/notification/{id}` |
| Current-user preferences | `GET|PUT /api/notification/preferences/me` |
| Current-user mute | `PUT /api/notification/preferences/me/mute` |
| Public Web Push config | `GET /api/notification/web-push/config` |
| Current browser subscription | `GET /api/notification/web-push/subscription?deviceIdentifier=...` |
| Register browser subscription | `POST /api/notification/web-push/subscriptions` |
| Remove browser subscription | `DELETE /api/notification/web-push/subscriptions/{subscriptionId}` |

Organization and Group preference matrices are exposed on their owning resource controllers.
Preference responses are HAL resources; Blazor renders save, mute, subscribe, and unsubscribe
actions only when the server emits the matching relation.

## UI And Navigation

The notification bell, panel, and full inbox page read durable inbox state. Opening the panel does
not mark rows read. The inbox supports paging, filters, item actions, and bulk mark-read.

`NotificationNavigationHelper` currently maps only:

- Organization notifications to `/organization/profile/{id}`; and
- Group notifications to `/group/profile/{id}`.

Unknown or unsupported entity types produce no guessed deep link. Item navigation is separate from
the explicit read-state operation.

## Refresh Stream

`GET /api/notification/stream` is an authenticated `text/event-stream` endpoint. It emits bounded
`notification-refresh` hints containing unread-count state, reason, and generation time.

The stream is a refresh signal, not delivery truth. Notification bodies and lifecycle state always
come from the authenticated list/detail APIs. The bell keeps polling as a fallback. Responses are
non-cacheable and disable proxy buffering.

## Actor Subscriptions

`ActorSubscription` persists the current tenant user's relationship to an Organization or Group
actor. Its API supports list, per-actor state, subscribe, notification-level update, and
unsubscribe. Unsubscribe preserves the durable row by changing status; optimistic concurrency
protects later changes.

Organization and Group profile buttons are gated by actor and subscription HAL relations. The
subscription is preference state only until a source-backed producer defines what occurrence
creates a notification, how it is deduplicated, and which delivery policy applies.

## Preference Matrix

Preference categories and channels are normalized lookup rows. Current-user, Organization, and
Group matrices store scoped channel choices and global mute state. Required categories remain
server-governed and cannot be disabled through client-only logic.

The matrix describes allowed delivery choices. It does not create an inbox producer or transport
workflow by itself.

## Web Push Boundary

Authenticated users can explicitly register or remove one browser subscription. The server stores
bounded Push API key material, applies ownership checks, and exposes enrollment only through HAL.
The browser UI treats denied permission as terminal until the user changes browser settings.

`WebPushDispatchOutbox`, its repository, drain service, and processor provide durable transport
infrastructure. Delivery uses generic refresh/navigation payloads, bounded TTL, topic coalescing,
same-origin navigation validation, and stale-subscription removal on push-service `404`/`410`.

Current source has no application producer that enqueues `WebPushDispatchOutbox` from a
`Notification`. Therefore enrollment and drain infrastructure must not be described as automatic
push delivery for inbox rows.

## Notification Intent And Account Authority

`DefaultNotificationOrchestrator` resolves ownership before recording a durable
`NotificationIntent`. Intents contain normalized category, owner, recipient, status, template key,
deduplication key, safe payload reference/hash, correlation, and optional user identity. They do
not store message bodies or credentials.

Current routing rules are explicit:

- identity-lifecycle email belongs to the configured account authority;
- provider-internal work may be delegated to an external workflow provider; and
- product-lifecycle, platform-operations, and marketing categories remain disabled until an owner
  is implemented.

The registered account-authority lifecycle service exposes email-verification, password-reset,
and email-update-verification requests. When called, it records local intent/delegation rows for
safe audit correlation and leaves credential-token generation and provider execution with the
account authority. No checked-in application handler or controller currently invokes that
service, so these registered components are not yet a live end-to-end product workflow. This
audit model is separate from the in-app `Notification` inbox.

## Email Boundary

Direct application email uses `EmailDispatchOutbox`, attempts, receipts, and tenant control state.
Account-authority lifecycle email is modeled as provider-owned delegation, with no current
application caller. Neither mechanism is created by merely persisting an in-app `Notification`
row.

Do not claim notification-to-email fanout, email digests, or transport receipts for inbox rows.
See [Email notifications](EMAIL_NOTIFICATIONS.md) for SMTP and account-authority details.

## Unsupported Claims To Avoid

- Permanent inbox deletion.
- Mark-as-unread.
- Automatic actor-subscription or content fanout.
- Automatic email or Web Push delivery from inbox rows.
- Public user-to-user actor subscriptions.
- SSE as content delivery or state authority.
- Entity deep links beyond the routes registered by `NotificationNavigationHelper`.

## Related

- [Email notifications](EMAIL_NOTIFICATIONS.md)
- [Outbox pattern](OUTBOX_PATTERN.md)
- [API conventions](API.md)
- [Authorization](AUTHORIZATION.md)
