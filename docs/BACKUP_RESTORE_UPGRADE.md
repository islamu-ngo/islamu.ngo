<!-- ABOUTME: Draft operator runbook for first-party ISLAMU backup, restore, upgrade, and rollback. -->
<!-- ABOUTME: Matches the production Compose topology and its external identity, cache, email, and policy providers. -->

# Backup, Restore, And Upgrade

> **Audience:** Operators
> **Status:** Draft
> **Owner:** Platform/Ops
> **Last Verified:** 2026-07-18
> **Source Anchors:** `compose.production.yaml`, `docs/PRODUCTION_OPERATIONS.md`, `src/Event.MigrationService/Worker.cs`, `src/Explore.API/Program.cs`, `docs/OPERATIONS.md`, `docs/CONFIGURATION.md`, `docs/SECRETS.md`

This runbook describes the source-controlled first-party production boundary. It is Draft because no environment-specific backup, restore, or rollback execution evidence is recorded here.

Do not start a release migration until the target environment has a verified PostgreSQL backup and restore procedure.

## Production Topology

compose.production.yaml defines six services in dependency order:

1. production-environment-validation
2. postgres
3. keycloak-ngo-provisioner
4. islamu-ngo-migration
5. islamu-ngo-api
6. islamu-ngo-ui

Only the UI/BFF publishes the application port. Production Keycloak, Redis-compatible cache, SMTP, Cerbos when selected, TLS/reverse proxy, and provider infrastructure are external. The repository Compose file owns two persistent volumes: postgres_data and local_storage_data.

The Event.MigrationService project name is a retained technical source identifier. The production service and product boundary are islamu-ngo-migration and the first-party ISLAMU website.

## Backup Inventory

| Asset | Production owner | Required evidence |
|---|---|---|
| Application PostgreSQL | postgres / postgres_data | Consistent database backup, checksum, retention, restore test |
| Local object storage | local_storage_data | Snapshot/copy matched to database recovery point |
| External Keycloak | Identity operator | Realm/users/clients/keys backup and restore procedure |
| External cache | Cache operator | Availability/recovery plan; document session and temporary-state loss behavior |
| External Cerbos, if selected | Authorization operator | Cerbos PostgreSQL/policy-store backup plus deployed policy commit |
| SMTP/email provider | Email operator | Configuration/credentials and provider-owned delivery/audit retention as applicable |
| Svix or other webhook provider | Integration operator | Provider application/binding configuration and provider-owned history as applicable |
| Secrets | Secret-provider operator | Versioned, access-controlled recovery procedure without plaintext in release artifacts |
| Release manifest | Release operator | Image digests, commit, configuration version, migration result, external provider versions |

Application PostgreSQL contains current tenants, organizations, groups, users, settings, Blog data, webhook ledgers, outbox state, provider configuration references, federation foundation records, and other live platform data. Retired product tables are not part of the expected inventory.

Docker image tags are not backups. Database, object storage, identity, secrets, and matching application/policy versions must be recoverable together.

## Pre-Backup Gate

1. Record the release commit and immutable image digests.
2. Record the resolved service list and non-secret configuration keys.
3. Record the external Keycloak, cache, SMTP, Cerbos, storage, and Svix ownership/versions in use.
4. Quiesce writes or use database/storage snapshot mechanisms that guarantee a consistent recovery point.
5. Back up PostgreSQL and local storage.
6. Trigger the owned external-provider backup procedures.
7. Encrypt artifacts, calculate checksums, and record retention/restore ownership.
8. Restore into an isolated environment and validate before declaring the backup usable.

An illustrative PostgreSQL backup invocation is:

~~~shell
docker compose -f compose.production.yaml exec -T postgres \
  sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' \
  > islamu-platform.dump
~~~

Treat this as a template: validate paths, roles, encryption, destination capacity, and error handling in the target environment. Never print database passwords.

## Restore

1. Declare the recovery point and incident scope.
2. Stop user traffic and application writes.
3. Verify artifact checksums, encryption access, release manifest, and target capacity.
4. Restore application PostgreSQL before starting dependent application services.
5. Restore local_storage_data to the matching recovery point.
6. Restore or verify external Keycloak. Re-running the repository provisioner is additive configuration, not a substitute for a Keycloak backup.
7. Restore or verify external Cerbos and republish the matching policy/schema bundle when Cerbos is selected.
8. Restore or verify external cache according to the incident. Cache loss may invalidate sessions and temporary state.
9. Reconcile external webhook/email/provider state according to provider ownership and idempotency rules.
10. Start the source-controlled chain in order: validation, PostgreSQL, Keycloak provisioner, migration, API, UI.
11. Validate the restored data and security boundaries before reopening traffic.

A database restore template for an empty, isolated target is:

~~~shell
docker compose -f compose.production.yaml exec -T postgres \
  sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists' \
  < islamu-platform.dump
~~~

Do not run a destructive restore against an unresolved database target. The exact database reset/ownership sequence belongs in the environment runbook and must be rehearsed.

## Post-Restore Validation

Validate at minimum:

- production-environment-validation completes;
- Keycloak NGO client provisioning completes without deleting unrelated realm resources;
- islamu-ngo-migration completes once;
- API and UI health/readiness match the release;
- login, logout, and tenant resolution work;
- anonymous first-party website and Blog reads work;
- tenant/organization/group writes remain isolated and HAL-authorized;
- representative storage objects match database records and access rules;
- application email can enqueue and dispatch without exposing content in logs;
- webhook consumers, endpoints, message evidence, and retry/redrive state are coherent;
- Cerbos decisions fail closed if Cerbos is selected;
- PDS/federation workers do not replay completed effects incorrectly.

## Upgrade

1. Review release notes, schema changes, policy changes, and external-provider compatibility.
2. Produce and verify the backup set.
3. Pin the new image digests and matching Cerbos policy commit.
4. Run production environment validation.
5. Allow the additive Keycloak provisioner to finish.
6. Run islamu-ngo-migration as the explicit one-shot migration service.
7. Start API only after migration succeeds.
8. Start UI only after API health succeeds.
9. Run the post-restore/upgrade validation set and monitor outbox, webhook, email, and federation workers.

Explore.API does not replace the explicit production migration service. If migration fails, stop the rollout; do not let API/UI traffic continue against an uncertain schema.

## Rollback

An image rollback is safe only when the database and external contracts remain backward compatible. If the migration ran, assume a database restore is required unless the release evidence proves otherwise.

Rollback together:

- application image digests;
- PostgreSQL and matching local storage recovery point;
- Cerbos schema/policy bundle when selected;
- compatible Keycloak client configuration;
- configuration and secret versions;
- external-provider compatibility state.

After rollback, repeat tenant isolation, HAL authorization, storage, email, webhook idempotency, and health validation.

## Local Development Is Different

docker-compose.yml and Aspire local profiles may provision Keycloak, Cerbos, MinIO, Svix, Mailpit, Coop, Osprey, and their databases. Those local technical provider semantics are not the production ownership model. Do not copy a local-volume inventory into a production recovery plan without resolving the actual external owners.

## Evidence Required To Activate This Runbook

- timestamped PostgreSQL backup and isolated restore result;
- local-storage recovery result matched to the database point;
- external Keycloak recovery evidence;
- selected Cerbos/provider recovery or re-publication evidence;
- end-to-end post-restore smoke results;
- upgrade and rollback rehearsal with measured recovery time and data-loss window;
- named operator ownership and artifact retention location.
