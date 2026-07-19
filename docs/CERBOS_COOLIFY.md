<!-- ABOUTME: Draft Coolify runbook for an external Cerbos PDP used by the first-party ISLAMU platform. -->
<!-- ABOUTME: Covers version alignment, PostgreSQL bootstrap, TLS, secrets, health, and policy publication. -->

# Cerbos On Coolify

> **Audience:** Operators
> **Status:** Draft
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `cerbos/config/.cerbos.yaml`, `cerbos/init/cerbos-schema.sql`, `cerbos/policies/`, `cerbos/tests/`, `docker-compose.yml`, `src/Explore.AppHost/AppHost.cs`, `.github/workflows/cerbos-policy-check.yml`, `compose.production.yaml`

Use this runbook when an external Cerbos Policy Decision Point (PDP) is operated through Coolify. The production ISLAMU Compose topology does not bundle Cerbos; selecting the Cerbos authorization provider therefore requires an independently owned PDP and Admin API destination.

This is a static, source-grounded recipe. It remains Draft until an operator records deployment, policy publication, authorization, backup, and rollback evidence for the target environment.

## Version And Ownership

Repository local orchestration and CI currently use Cerbos/Cerbosctl 0.53.0. Deploy the same reviewed version:

| Component | Version |
|---|---|
| ghcr.io/cerbos/cerbos | 0.53.0 |
| ghcr.io/cerbos/cerbosctl | 0.53.0 |

Do not use latest. Prefer a digest-pinned image after validating the digest in the deployment environment. Upgrade the server, policy compiler/test job, and publication client intentionally.

The first-party platform owns:

- the islamuevent_* resource/action contract;
- policy and schema source under cerbos/policies;
- policy tests under cerbos/tests;
- runtime provider selection and fail-closed behavior.

The external Cerbos deployment owns:

- server availability and TLS;
- Admin API credentials;
- PostgreSQL policy-store backup/restore;
- audit-log handling;
- network access from the API and CI publication job.

The islamuevent_* names are technical policy identifiers and must be preserved. Retired product-domain resources are not valid current policies.

## Coolify Resources

Create:

1. A PostgreSQL database dedicated to Cerbos policy storage.
2. A Cerbos Docker Image application using version 0.53.0.
3. A persistent, operator-owned secret set.
4. Internal HTTP/gRPC connectivity and an externally protected Admin API path only where CI publication requires it.

Initialize the database with cerbos/init/cerbos-schema.sql. The repository configuration expects the cerbos search path and a PostgreSQL storage driver.

Mount or render cerbos/config/.cerbos.yaml at /config/.cerbos.yaml and start:

~~~text
server --config=/config/.cerbos.yaml
~~~

The repository file enables the Admin API, PostgreSQL storage, audit logging, schema enforcement in warn mode, and lenient scope search. Review those settings for production; warn mode is rollout behavior, not strict schema rejection.

## Required Configuration

Provide the equivalent of:

| Setting | Purpose |
|---|---|
| CERBOS_ADMIN_USER | Admin API username |
| CERBOS_ADMIN_PASSWORD_HASH | Base64 bcrypt hash consumed by the Cerbos server |
| CERBOS_PG_URL | PostgreSQL URL including the cerbos search path and production TLS mode |

The plaintext Admin API password is required only by trusted publication/reconciliation clients. It is not the same value as the bcrypt hash environment variable.

Generate the bcrypt value in a controlled environment, encode it as required by the repository configuration, and store both server hash and client plaintext in separate Coolify/CI secret boundaries. Never commit either value.

## Networking And TLS

Cerbos listens on HTTP 3592 and gRPC 3593 inside the container.

- The ISLAMU API runtime uses the gRPC endpoint for decisions.
- Policy publication uses the Admin API.
- Health monitoring uses /_cerbos/health.

Production traffic must not use the local plaintext-mode defaults. Prefer private service networking plus TLS/mTLS appropriate to the deployment. If the Admin API is reachable outside the private network, place it behind authenticated, allow-listed ingress and a trusted certificate chain.

Configure the ISLAMU API with the external gRPC endpoint and the matching TLS mode. Configure Admin API endpoints and credentials only where policy synchronization is enabled. A healthy HTTP endpoint does not prove gRPC decisions or correct policies.

## Policy Validation And Publication

Before publication:

1. Compile cerbos/policies with Cerbos 0.53.0.
2. Run the native policy tests in cerbos/tests.
3. Confirm retired product-domain resource names are absent.
4. Confirm every live ResourceKinds value that requires Cerbos has an owned policy path.
5. Publish schemas and policies using Cerbosctl 0.53.0.
6. Reload the store and retain publication evidence without secret output.

The repository workflow .github/workflows/cerbos-policy-check.yml implements version-aligned compilation/testing and an authenticated publication job. Repository workflow configuration is not proof that a target Coolify instance was reached.

## Verification

Record all of the following before changing this document to Active:

- server and PostgreSQL health;
- TLS verification from the API network;
- successful policy/schema publication;
- an allowed current resource decision;
- a denied current resource decision;
- denial when Cerbos is unavailable;
- tenant/provider routing behavior;
- audit output that contains no tokens, credentials, or unnecessary resource attributes;
- database backup and restore;
- rollback to the previous server and policy bundle.

## Failure And Rollback

Cerbos unavailability or an invalid decision must fail closed. Do not switch to permissive local rules as an availability workaround.

Rollback is two-dimensional:

- restore the previously validated Cerbos server image;
- restore or republish the matching schema/policy bundle.

Keep the PostgreSQL backup, deployed image digest, policy commit, publication evidence, and ISLAMU release manifest together. See [AUTHORIZATION.md](AUTHORIZATION.md) and [BACKUP_RESTORE_UPGRADE.md](BACKUP_RESTORE_UPGRADE.md).
