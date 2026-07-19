<!-- ABOUTME: Documents the current federation foundations and protocol boundary of the ISLAMU platform. -->
<!-- ABOUTME: Separates internal resources and outbound PDS sync from unimplemented public federation protocols. -->

# Federation

> **Audience:** Contributors | Integrators | AI agents
> **Status:** Active
> **Owner:** API
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Domain/Actor.cs`, `src/Explore.Domain/AtprotoRecord.cs`, `src/Explore.Domain/IndexedDid.cs`, `src/Explore.Domain/ActorKeyStore.cs`, `src/Explore.Domain/Federation/PdsSyncOutbox.cs`, `src/Explore.API/Controllers/AtprotoRecordController.cs`, `src/Explore.API/Controllers/IndexedDidController.cs`, `src/Explore.API/Controllers/ActorKeyStoreController.cs`, `src/Explore.API/BackgroundServices/PdsSyncWorker.cs`, `src/Explore.Infrastructure/Services/Federation/PdsService.cs`, `src/Explore.Blazor/Authentication/AtprotoAuthenticationHandler.cs`

## Current Boundary

The first-party ISLAMU platform contains federation data and outbound synchronization foundations. It is not currently a public ATProto PDS, ATProto OAuth provider/client, ActivityPub server, WebFinger endpoint, or AppView.

| Capability | Current state |
|---|---|
| Actor, AtprotoRecord, IndexedDid, ActorKeyStore, PdsSyncOutbox models | Implemented |
| EF Core configurations, DbSets, and repositories | Implemented |
| Internal API resources for ATProto records and actor keys | Implemented, authenticated |
| Indexed DID reads | Implemented, anonymous |
| Indexed DID writes | Implemented, authenticated and resource-authorized |
| Outbound PDS sync worker | Implemented |
| XRPC putRecord and deleteRecord calls to a configured PDS | Implemented |
| Public ATProto OAuth/login | Not implemented |
| Hosted PDS protocol endpoints | Not implemented |
| ActivityPub/WebFinger bridge | Not implemented |
| Public AppView/query/indexing service | Not implemented |

The internal CRUD controllers are platform API resources. Their presence must not be represented as protocol conformance.

## Data And Synchronization Flow

~~~text
authorized application change
  -> AtprotoRecord and/or PdsSyncOutbox persistence
  -> PdsSyncWorker polls pending outbox rows
  -> PdsService calls the configured PDS XRPC endpoint
  -> success/failure state is recorded for subsequent processing
~~~

PdsService currently calls:

- com.atproto.repo.putRecord
- com.atproto.repo.deleteRecord

The target PDS host and credentials are integration configuration. The remote PDS remains the protocol/account authority for the records it accepts.

## Authentication Truth

AtprotoAuthenticationHandler is an explicit placeholder. It returns no authenticated identity and responds to an ATProto challenge with HTTP 501. Therefore:

- auth.atproto_login_enabled is governance/configuration state, not evidence of a working login flow;
- no browser or API caller can be documented as authenticated through ATProto;
- public OAuth, DID proof, token validation, session refresh, account recovery, and logout semantics remain unimplemented.

When a real flow is added, account verification and recovery email belongs to the PDS or account authority that owns that identity. ISLAMU application email must not impersonate that authority.

## Authorization And Tenant Safety

Current platform resource identifiers include islamuevent_actor, islamuevent_actor_subscription, islamuevent_atproto_record, and islamuevent_indexed_did. These are stable technical identifiers, not product names.

- ATProto record and actor-key controllers require authentication.
- Indexed DID GET routes are anonymous; writes require authentication and resource authorization.
- HAL links remain the UI authority for available actions.
- Outbound sync must preserve the tenant/actor/record ownership established by the application.
- Remote PDS credentials must never be selected from an untrusted record body or request header.
- Actor private keys and provider credentials must stay in the configured secret/storage boundary and out of DTOs and logs.

The local fallback authorization service is intentionally conservative for federation writes. Do not broaden it merely to make a protocol experiment succeed.

## Governance Settings

The platform retains federation.decentralization_enabled and auth.atproto_login_enabled settings, including governance locks. These settings let the first-party control plane govern future activation. They do not make an absent route or protocol implementation available.

Any UI must distinguish:

- configured or permitted;
- technically available;
- successfully connected.

## Explicitly Removed Product Projections

Retired product lexicons, data hierarchies, AppView examples, discovery routes, registration federation, and product-specific CID/indexing tables are not current source truth. They must not be used as examples for the retained generic federation foundation.

No new lexicon or public protocol claim is active until it has:

- an owned schema and source implementation;
- authentication and authorization rules;
- tenant and key-custody design;
- interoperability and failure-mode tests;
- privacy, deletion, and replay semantics;
- an operator rollout/rollback boundary.

## Verification

Repository checks can prove models, controllers, workers, XRPC methods, and placeholder authentication behavior. End-to-end federation requires a configured external PDS and protocol conformance evidence; this documentation cleanup did not start services or perform deployment verification.
