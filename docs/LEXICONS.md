<!-- ABOUTME: Documents the current federation collection-identity and lexicon boundary. -->
<!-- ABOUTME: Separates opaque ATProto record metadata from unimplemented ISLAMU schema promises. -->

# Federation Collection And Lexicon Boundary

> **Audience:** Contributors | Integrators | AI agents
> **Status:** Active
> **Owner:** Architecture/API
> **Last Verified:** 2026-07-19
> **Source Anchors:** `docs/FEDERATION.md`, `src/Explore.Domain/AtprotoRecord.cs`, `src/Explore.Domain/Federation/PdsSyncOutbox.cs`, `src/Explore.Application/DTOs/AtprotoRecord/Validators/CreateAtprotoRecordDtoValidator.cs`, `src/Explore.Infrastructure/Services/Federation/PdsService.cs`, `src/Explore.API/BackgroundServices/PdsSyncWorker.cs`

## Current Truth

There is no checked-in ISLAMU-owned lexicon registry, schema bundle, generated lexicon model, or
stable `im.islamu.*` compatibility contract in current source.

The federation foundation stores and transports record identity supplied to it:

| Field | Current meaning |
|---|---|
| `Did` | Actor DID that owns or identifies the remote record |
| `Collection` | Opaque collection identifier passed to the configured PDS |
| `RecordKey` | Record key inside that collection |
| `Cid` | Optional content identifier returned or indexed for the record |
| `Uri` | Optional record URI |
| `Payload` | JSON payload carried by outbound `PdsSyncOutbox` create/update work |

`AtprotoRecord` persists DID, collection, key, CID, URI, and indexed timestamp. The create DTO
requires DID, collection, and record key and applies bounded lengths; it does not prove NSID syntax
or validate an ISLAMU schema.

`PdsSyncOutbox` adds operation, payload, target host, source correlation, retry, and terminal state.
`PdsSyncWorker` passes due rows to `PdsService`, which calls `com.atproto.repo.putRecord` or
`com.atproto.repo.deleteRecord` on the configured PDS.

## Authority Boundary

The configured remote PDS is authoritative for whether a collection identifier and record payload
are accepted. Local persistence or a successful internal CRUD request does not establish:

- an ISLAMU lexicon standard;
- public ATProto protocol conformance;
- a hosted PDS or AppView;
- cross-version schema compatibility;
- discovery, search, ranking, or federation availability; or
- a public product projection.

The platform must treat collection names and record payloads as untrusted integration data at
network, logging, and authorization boundaries. Credentials and target hosts come from approved
configuration, never from an untrusted record body.

## Adding An Owned Lexicon

An ISLAMU-owned lexicon becomes active only after the same change supplies:

1. a versioned schema file with an identified owner;
2. generated or typed source models and validation;
3. explicit authentication, authorization, tenant, and DID-custody rules;
4. create, update, deletion, replay, and failure semantics;
5. interoperability and protocol tests against the configured PDS;
6. privacy and data-retention review; and
7. rollout and rollback instructions.

Until those artifacts exist, do not reserve a namespace, mark an identifier stable, publish an
add-only promise, or use a hypothetical lexicon as a database/domain design authority.

## Evolution Rules After Activation

If a lexicon is introduced later, its owner must define compatibility rules in the schema itself.
At minimum:

- additive optional fields may remain within an explicitly versioned compatibility line;
- deletion, renaming, requiredness changes, or tighter constraints require a reviewed breaking
  version decision;
- experimental identifiers must be clearly labeled and must not be used as stable public keys;
- generated models and protocol fixtures must be regenerated from the schema; and
- old versions remain supported only when an active consumer and explicit compatibility window
  justify them.

These are governance requirements for a future owned contract, not evidence that one exists now.

## Related

- [Federation](FEDERATION.md)
- [Architecture](ARCHITECTURE.md)
- [API conventions](API.md)
