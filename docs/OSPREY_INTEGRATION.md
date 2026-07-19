<!-- ABOUTME: Documents the retained Osprey provider infrastructure in the first-party ISLAMU platform. -->
<!-- ABOUTME: Distinguishes the provisioned coordinator and protocol contracts from absent application workflows. -->

# Osprey Integration

> **Audience:** Operators | Integrators | Contributors | AI agents
> **Status:** Active
> **Owner:** Platform/Integrations
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.AppHost/AppHost.cs`, `src/Explore.Infrastructure/Protos/osprey/`, `src/Explore.Infrastructure/Explore.Infrastructure.csproj`, `.env.example`

## Current Truth

Osprey is an optional, locally provisioned provider foundation. The current application does not expose an Osprey-backed moderation or classification capability.

| Capability | Current source state |
|---|---|
| Osprey coordinator container | Provisioned in the FullLocal Aspire topology. |
| Kafka broker and actions-input topic | Provisioned for the local coordinator. |
| Coordinator TCP endpoints | Container ports 19950 and 19951, mapped locally to 29950 and 29951. |
| gRPC protocol contracts | Retained under src/Explore.Infrastructure/Protos/osprey and generated as client contracts by the Infrastructure project. |
| Application gRPC adapter | Not present. |
| Compatible HTTP reporting facade | Not present. |
| Active provider | Disabled by default; .env.example explicitly requires a facade or gRPC adapter first. |
| Public callback, report, policy-simulation, or moderation route | Not present. |
| Blazor moderation UI or premium policy controls | Not present. |

The coordinator image and generated protocol types prove that a technical integration boundary is retained. They do not prove that platform data is submitted to Osprey or that verdicts affect any first-party resource.

## Local Provider Infrastructure

FullLocal orchestration starts:

1. a single-node Kafka resource;
2. a bootstrap job that creates the osprey.actions_input topic;
3. the Osprey coordinator configured for Kafka input;
4. coordinator bidirectional-stream and synchronous-action TCP endpoints.

The repository environment template describes this coordinator as different from the HTTP-compatible Reporting:Osprey facade expected by older configuration. Reporting:Osprey:Enabled therefore defaults to false even when the container exists.

These are technical self-hosted provider semantics only. They do not make the first-party ISLAMU website a self-hostable product and do not establish production readiness. The default latest image reference must be replaced by a reviewed, pinned version before production use.

## Tenant Isolation

No current adapter establishes how Osprey state, rules, subjects, or verdicts would be isolated per tenant. Until such an adapter exists:

- do not send platform payloads to the coordinator;
- do not assume a request field can establish tenant authority;
- do not share stateful reputation or policy data across tenants;
- do not treat provider verdicts as authorization decisions;
- do not persist or execute provider actions without an application-owned, idempotent CQRS workflow.

If Osprey cannot provide hard tenant isolation for a stateful feature, the application must either isolate provider instances or keep the relevant state and policy evaluation inside the platform boundary.

## Activation Requirements

Before Osprey can be documented as implemented end to end, source must add and verify:

- an owned gRPC adapter or compatible HTTP facade;
- tenant-bound provider configuration and credentials;
- a current platform resource and payload contract;
- timeouts, cancellation, retry classification, and circuit behavior;
- durable, idempotent recording of requests and verdicts;
- explicit separation between advisory safety signals and authorization;
- redaction rules for personal or sensitive content;
- production version, health, backup, and rollback evidence;
- cross-tenant and failure-mode integration tests.

Retired report, moderation-queue, policy-simulation, and automatic-escalation projections are not current capabilities and must not be described as available.
