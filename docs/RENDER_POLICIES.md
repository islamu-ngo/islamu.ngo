<!-- ABOUTME: Current Blazor route classification and prerender policy for the first-party ISLAMU website. -->
<!-- ABOUTME: Distinguishes implemented InteractiveServer behavior from stored governance options and planned changes. -->

# Render Policies

> **Audience:** Frontend contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Frontend
> **Last Verified:** 2026-07-18
> **Source Anchors:** `src/Explore.Blazor.Client/Services/RuntimeRenderPolicyService.cs`, `src/Explore.Blazor.Client/Routes.razor`, `src/Explore.Blazor.Client/Pages/Blogs/`, `src/Explore.Application/DTOs/Instance/RenderPolicySettingsDto.cs`

## Current Runtime Rule

`RuntimeRenderPolicyService` resolves a route group and a prerender flag. It currently returns
`InteractiveServer` for every group, regardless of stored render-mode values. Stored governance
settings still influence prerender selection.

This is the source-backed runtime contract. Do not describe InteractiveAuto or WebAssembly as
active route outcomes merely because related enums/settings remain in the codebase.

## Route Groups

The classifier normalizes paths to lowercase and removes a trailing slash.

| Group | Current path rules |
|---|---|
| `Onboarding` | `/setup`, `/startup`, `/onboarding/*` |
| `Admin` | `/admin/*` |
| `PublicSeo` | `/`, `/welcome`, `/home` |
| `Operational` | Every other route |

### Blog

`/blog`, `/blog/{slug}`, and `/blog/manage*` currently classify as `Operational`. The Blog Razor
components explicitly declare `InteractiveServer`; authenticated management routes also use route
guards/authorization and `noindex,nofollow` metadata.

Blog is implemented, but it is not currently included in the `PublicSeo` render-policy group.

### Navigation

`SiteMegaMenu` is part of the shared shell and does not make per-destination render decisions. It
navigates to existing routes; each destination follows its own component and runtime policy.

## Prerender Resolution

1. If settings cannot be loaded, `PublicSeo` defaults to prerender enabled and all other groups to
   prerender disabled.
2. With settings, global prerender is the base value.
3. When advanced overrides are enabled, the selected route group's prerender flag wins.
4. When advanced overrides are disabled and preset is `SeoBalanced`, `PublicSeo` prerender is
   enabled.
5. The resulting render mode is still `InteractiveServer`.

## Settings Source

`PublicExperienceSettingsDto` is loaded through `IPublicExperienceService`. Effective values are
produced by the hierarchical settings resolver and current instance/tenant governance rules.

Relevant stored keys remain under `routing.render_policy.*`, including:

- preset and advanced-enabled flags;
- global prerender;
- public SEO, operational, admin, and onboarding prerender flags; and
- tenant-override/lock settings.

These keys are implementation inputs. They do not prove every legacy selectable render mode is
currently honored by the runtime resolver.

## Current Public Routes

`Explore.Blazor.Client/Routes.razor` registers implemented Blog routes:

- `/blog`
- `/blog/:Slug`
- `/blog/manage`
- `/blog/manage/new`
- `/blog/manage/:Id`

The route table also contains current public/legal, account, organization/group, setup, and admin
destinations. Product documentation and public navigation should expose only implemented routes.

## Safe Authoring Rules

1. Read `RuntimeRenderPolicyService` and the target Razor component before documenting render mode.
2. Do not infer runtime behavior from enum values or settings DTOs alone.
3. Keep onboarding/admin routes out of public SEO claims.
4. A new public content route needs route registration, title/metadata, indexing decision,
   accessibility review, and sitemap decision.
5. If route classification changes, update [SEO.md](SEO.md) in the same slice.

## Known Gap

Blog reader routes have canonical metadata and sitemap entries but are not classified as
`PublicSeo`. This docs lane records the mismatch; it does not change product source or claim that
`SeoBalanced` prerenders Blog pages.

## Related

- [BLAZOR.md](BLAZOR.md)
- [SEO.md](SEO.md)
- [PROJECT.md](PROJECT.md)
- [ACCESSIBILITY.md](ACCESSIBILITY.md)
