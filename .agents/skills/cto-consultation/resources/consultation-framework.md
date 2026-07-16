<!-- ABOUTME: Atomic consultation framework for CTO-level ISLAMU website product and architecture decisions. -->
<!-- ABOUTME: Provides decision lenses, option scoring, and repo-specific patterns for strategic advice. -->

# CTO Consultation Framework

## Consultation Contract

Start every consulting answer by identifying what kind of decision is being made:

1. Product direction: audience, positioning, roadmap, packaging, differentiation.
2. Architecture direction: layer ownership, contracts, data model, operational lifecycle.
3. Infrastructure posture: required dependency, retained first-party service, managed-provider path, or deferred capability.
4. UX/workflow design: who acts, what they see, what authorization source gates the action.
5. Compliance and convention: privacy, retention, audit, accessibility, localization, security, and first-party operations.

Then state whether the answer is based only on repository sources or also on current external research. Use external research for current market, competitor, vendor, legal, regulatory, pricing, or library-status claims, and cite dates/sources in the final recommendation.

## Product Thesis To Preserve

ISLAMU is a first-party digital front door. Marketing is the entry layer: orient visitors around purpose, publish trustworthy multilingual content, offer only real participation paths, and create a governed foundation for future services.

Advice must preserve purpose before promotion, editorial depth, English/French/Arabic and RTL parity, WCAG 2.2 AA, calm interaction, privacy-respecting analytics, operational resilience, and truthful Implemented/Planned/Deferred boundaries. “Much more” is a capability horizon, not permission to invent routes, forms, dependencies, or public promises.

## Repository Decision Lenses

Use these lenses before recommending a feature shape.

| Lens | Ask |
|---|---|
| Authority | Is the source of truth content governance, API, BFF, client, PostgreSQL, external provider, or a derived projection? |
| Horizon | Is this Foundation, Publish, Engage, or Expand, and is it Implemented, Planned, Deferred, or Historical? |
| Public truth | Is there a real route, content owner, approved claim, locale parity, and working destination? |
| Architecture | Which Clean Architecture layer owns the behavior and which external boundary does it cross? |
| Affordance | Is the UI action exposed by HAL `_links`, a BFF status endpoint, or a non-authoritative visual hint? |
| Operator burden | Does it add a required service, secret, migration, backup path, health check, retention policy, or recovery runbook? |
| Isolation | Does it preserve every authorization and data-isolation boundary still required by surviving source? |
| Durability | Is this durable business state, rebuildable projection, compliance evidence, or ephemeral cache? |
| Interoperability | Does it affect OpenAPI, generated client, federation lexicons, webhooks, import/export, or external API keys? |
| Governance | Who owns content, configuration, incident response, retention, and removal? |

## Recommendation Format

A complete consultation should include:

1. Decision: one sentence naming the actual choice.
2. Current facts: repo-grounded facts from docs/code, not generic architecture theory.
3. Options: usually 2-4 realistic options, including "defer" when valid.
4. Recommendation: a clear default, not an endless list.
5. Rationale: tradeoffs across product, architecture, security, operations, and UX.
6. Implementation path: phased slices that can be tested and documented.
7. Risks and reversibility: what can fail, what is hard to undo, and what must be measured.
8. Verification: tests, architecture checks, health checks, docs, or external evidence needed.

## Option Scoring

Score options qualitatively as Low, Medium, or High across:

- user value;
- implementation complexity;
- operational complexity;
- security/privacy risk;
- accessibility/localization quality;
- first-party operational burden;
- reversibility;
- market differentiation.

Avoid recommending a high-complexity/high-operator-burden path unless it unlocks clear public value or removes a real product risk.

## Infrastructure Optionality Pattern

When deciding whether infrastructure should be required, optional, or hidden:

1. Required only if the core product cannot work safely without it.
2. Retained optional service if current callers need it and first-party operations own its health, fallback, secrets, and recovery.
3. Internal implementation detail if users should not manage it directly.
4. Product admin surface if operators need stable business state, replay, audit, configuration, or remediation.
5. Infrastructure dashboard only for instance operators and never as the product source of truth.

Example: TickerQ can be a default scheduler trigger for Basic Dispatch Mode, but its dashboard should remain disabled by default, instance-admin-only, and secondary to EmailDispatchOutbox state, health checks, metrics, and HAL-gated product admin APIs.

## Website Analytics Pattern

For website analytics, separate:

- operational analytics: health and delivery outcomes without personal payloads;
- discovery analytics: pageviews and explicitly approved low-risk interactions;
- governance: provider, consent mode, relay/proxy/direct transport, retention, and global disable path;
- privacy/compliance: pseudonymous defaults, consent-driven identified analytics, no raw PII in metrics tags;
- UX authorization: dashboard availability from API/HAL or server-confirmed status, not local role checks.

A prudent path is usually:

1. Start disabled and define the event inventory, purpose, owner, retention, and consent posture.
2. Enable only low-risk page and journey measurements needed for a named product decision.
3. Use the existing PostHog integration only after privacy and deployment settings are approved.
4. Add richer identity or non-essential capture only through a separate reviewed workstream.

## Market And Competition Analysis

For market positioning, compare against categories rather than only named products:

- purpose-led nonprofit and institutional websites;
- multilingual editorial and knowledge platforms;
- community participation and membership journeys;
- trustworthy donation, program, and campaign experiences;
- accessible public-service and humanitarian digital products.

Evaluate competitors by:

- mission clarity and credibility;
- content depth and editorial governance;
- API and integration quality;
- participation journey quality;
- discovery/SEO strength;
- compliance and audit features;
- extensibility model;
- total operational burden;
- accessibility/localization maturity.

Because the market changes, verify current competitor capabilities, pricing, licensing, and acquisition/vendor status before making external claims.

## Legal And Convention Framing

Do not present legal advice. Present engineering controls and professional conventions:

- data minimization, consent, retention, export, withdrawal, audit evidence;
- administrator role separation and least privilege;
- security headers, CSRF protection, token handling, secret ownership, and redaction;
- accessibility baseline such as WCAG AA-aligned implementation and testing;
- localization and RTL support as product quality, not afterthought;
- first-party backup, restore, upgrade, incident, and rollback paths;
- AGPL/open-source implications as prompts for counsel review when commercial hosting, plugins, or private modifications are discussed.

When legal/regulatory conclusions depend on jurisdiction or current law, recommend consulting qualified counsel and provide the technical decision record counsel needs to review.

## Implementation Advice Boundaries

Consultation may propose implementation slices, but it should not silently bypass the Contribution Contract. If the user asks to proceed with implementation:

1. classify the implementation intent in `.claude/contract/intents.yaml`;
2. load the relevant docs, rules, and implementation skills;
3. keep changes within scoped paths;
4. update docs where the intent requires it;
5. run the minimum tests and architecture checks.

Strategic advice is useful only if it can become a coherent sequence of governed repository changes.
