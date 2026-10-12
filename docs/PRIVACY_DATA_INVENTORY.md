<!-- ABOUTME: Source-backed inventory of website data flows that may require privacy or consent treatment. -->
<!-- ABOUTME: Gives product, engineering, and legal owners facts without pretending to be a public privacy policy. -->

# Privacy Data Inventory

> **Audience:** Product and content owners | Security | First-party operators | Contributors
> **Status:** Active
> **Owner:** Security
> **Last Verified:** 2026-07-15
> **Source Anchors:** `src/Explore.Domain/Settings/Definitions/AnalyticsSettingDefinitions.cs`, `src/Explore.Infrastructure/Analytics/`, `src/Explore.Blazor/Services/ServerCookieConsentInterop.cs`, `src/Explore.Blazor.Client/Services/CookieConsentStateService.cs`, `src/Explore.Blazor/Services/ExploreBffCookieSessionHandler.cs`, `src/Explore.Infrastructure/Mail/`, `src/Explore.Infrastructure/EmailDispatchDrainService.cs`, `src/Explore.Infrastructure/Integrations/Listmonk/`, `docs/CONFIGURATION.md`, `docs/EMAIL_NOTIFICATIONS.md`, `docs/SECRETS.md`

This is an engineering inventory, not legal advice or a published privacy policy. ISLAMU must not publish privacy, cookie, retention, or lawful-basis claims until an accountable owner and legal reviewer approve wording against deployed configuration and actual data processors.

## Current Data Surfaces

| Surface | Current Source-Backed Behavior | Default / Boundary | Publication Gate |
|---|---|---|---|
| Authentication session | The BFF owns authentication cookies and server-side token handling. Browser code is not the token authority. | Strictly necessary for authenticated flows that survive the product refactor. | Document exact cookies, duration, logout, and identity processor after deployment configuration is fixed. |
| Analytics | Runtime provider abstraction supports PostHog and other configured providers; delivery failures are non-blocking. | `analytics.enabled=false` and provider `none` are clean disable paths. | Name the deployed provider, region, processor relationship, events, retention, and user controls before enabling production tracking. |
| Consent preference | The client and BFF include consent-state services and governance settings. | A consent preference cookie may remember the visitor's choice; non-essential PostHog features default off. | Verify banner behavior and withdrawal path in the deployed website before making public claims. |
| PostHog | Supports anonymous, pseudonymous, and identified modes plus direct, proxy, or relay transport. | Session replay, autocapture, heatmaps, and toolbar are disabled by default; person profiles default to identified-only. | Require an explicit owner decision for identity mode, cookieless mode, enabled events, and every non-essential feature. |
| Email delivery | Durable email outbox and SMTP delivery can send explicit transactional workflows. Mailpit captures local/test mail only. | Mailpit is not a production mailbox or processor. SMTP credentials are secrets. | Publish purpose, sender, recipient source, retention, unsubscribe requirements, and provider only for implemented workflows. |
| Listmonk | A durable integration path exists for provider synchronization. | Retained pending caller evidence; not proof of a public newsletter or waitlist. | A separate workstream must define consent evidence, double opt-in where required, unsubscribe, deletion, suppression, and processor configuration. |
| Localization | Weblate is the retained first-party translation workflow; offline bundles provide resilience. | Public content should not contain personal data. | Confirm translator access and processor/hosting treatment before documenting it publicly. |
| Operational logs and health | Application logs, traces, metrics, and health checks support operations. | Secrets, tokens, raw email content, and unnecessary personal data must not be logged. | Define deployed retention and access controls before publication. |

## Capability-Horizon Rules

- **Foundation and Publish:** collect no additional personal data merely to make the website interactive.
- **Engage:** waitlists, newsletters, contact forms, and transactional messages require a separate approved data-flow record before a route or CTA is published.
- **Expand:** donations, programs, personalization, community accounts, and other services are discovery-only until their own security/privacy design is approved.
- Planned or Deferred forms are not rendered as dead or data-collecting placeholders.
- Analytics is never a prerequisite for core content, navigation, localization, or accessibility.

## Required Record For A New Data Flow

Before implementation, record: purpose; accountable owner; data fields; source; recipients/processors; storage location; retention/deletion; access control; consent or other approved basis; withdrawal/unsubscribe path; security controls; failure behavior; analytics events; user-facing copy; and verification evidence.

## Publication Decision

The root README intentionally has no privacy-policy link today. Add a public privacy/cookie page only after this inventory matches the deployed system and owner/legal review supplies approved text. Never convert this engineering inventory directly into legal copy.
