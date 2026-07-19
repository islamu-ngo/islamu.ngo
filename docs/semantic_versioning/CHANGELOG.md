ABOUTME: Historical semantic-version index for the removed ISLAMU Event platform.
ABOUTME: Preserves the imported beta baseline and roadmap without presenting either as current product truth.

# Historical Changelog — ISLAMU Event Platform

> **Audience:** Contributors | Maintainers
> **Status:** Historical
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `docs/PROJECT.md`, `dev/report/EVENT_REFERENCE_ALIGNMENT.md`

> All notable changes are documented in version files.
> This project follows [Semantic Versioning 2.0.0](https://semver.org/).

This directory preserves imported Event release planning. The Event product has been removed, no
version listed here is the current ISLAMU website release or roadmap, and `TBD` release dates are
not evidence that a release occurred. Use [PROJECT.md](../PROJECT.md) for current product scope.

- **Historical Snapshot Updated:** 2026-03-27
- **Boundary Reviewed:** 2026-07-18

---

## Historical Unreleased Snapshot

Features recorded as merged to `develop` in the former Event release plan:

- **Outbox pattern** — transactional outbox with `OutboxProcessor` background service, exponential backoff retry, dead-letter queue, optimistic concurrency. Specialized variants: `PdsSyncOutbox`, `PolicyChangeOutbox`.
- **Footer management** — tenant-configurable footer with link groups, social links, 4 templates (standard-3-col, standard-2-col, minimal, community), instance governance locking. 11 API endpoints + admin UI.
- **Design system** — CSS `@layer` architecture (6 layers), 3-tier design token system, MudBlazor wrapper components (AppButton, AppCard, AppTextField, AppIconButton, AppDialogShell), `DialogOptionsFactory` presets, MudBlazor override whitelist policy.
- **Accessibility services** — `IAccessibilityAnnouncerService` (ARIA live regions), `IAccessibilityFocusService` (focus management with save/restore), JS interop module, `MainLayout` page shell with skip-link and landmarks, architecture convention tests (8 tests).
- **Secrets library** — `Explore.Secrets` multi-provider secret management (Environment, Infisical; Vault/Azure/AWS planned). Background refresh with exponential backoff, AES-256-GCM encryption, health checks, Prometheus metrics.
- **Actor appearance** — BackgroundColor, BackgroundEffect, BannerColor, BannerPictureId, BackgroundImageId fields. `AppearanceStyleBuilder` for inline CSS generation with overlay effects.
- **Analytics relay rate limiting** — dedicated `AnalyticsRelay` rate limit policy for `POST /api/a/t`.
- **Authorization parity tests** — architecture tests ensuring resource kinds map to Cerbos policies with fallback cases.

---

## Version History

| Version | Title | Released | Status |
|---------|-------|----------|--------|
| [v0.1.0](v0.1.0.md) | First Public Release (Beta) | TBD | Historical unreleased baseline |
| [v1.0.0](v1.0.0.md) | Planned Stable Release | TBD | Historical plan; not current roadmap |

---

## Versioning Policy

### Historical Phase: Pre-1.0 Beta (0.1.0)

- **Major version zero (0.y.z)** indicates initial development.
- **Implemented baseline for v0.1.0** is tracked in `v0.1.0.md` under **Implemented (v0.1.0)**.
- **Planned scope for v1.0.0** is tracked in `v1.0.0.md` under **Planned (v1.0.0)**.

### Historical Progression to 1.0.0

The former Event plan intended to move to **v1.0.0** after backlog completion and release
validation. That intent was not carried forward to the current ISLAMU website.

---

## Feature Checklist Mapping

### Historical v0.1.0 Baseline

- Source of truth: [v0.1.0.md](v0.1.0.md)
- Imported source recorded at the time: `API WORK ITEMS.md` (no longer present).

### Historical v1.0.0 Plan

- Source of truth: [v1.0.0.md](v1.0.0.md)
- Imported source recorded at the time: `API WORK ITEMS TODO.md` (no longer present).

---

## Notes

- The imported Event baseline classified implemented work under `v0.1.0`.
- The imported Event plan classified its enterprise backlog under `v1.0.0`; this is not a current
  ISLAMU commitment.
