<!-- ABOUTME: Source-backed SEO and public-discovery behavior for the first-party ISLAMU website. -->
<!-- ABOUTME: Covers current Blog metadata, sitemap, robots, render boundaries, and explicit gaps. -->

# SEO And Public Discovery

> **Audience:** Product/content owners | Contributors | First-party operators
> **Status:** Active
> **Owner:** Frontend | Content
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.API/Controllers/SitemapController.cs`, `src/Explore.Infrastructure/Services/PublicUrlBuilder.cs`, `src/Explore.Blazor/Controllers/RobotsController.cs`, `src/Explore.Blazor.Client/Pages/Blogs/BlogIndex.razor`, `src/Explore.Blazor.Client/Pages/Blogs/BlogArticle.razor`, `src/Explore.Blazor.Client/Services/RuntimeRenderPolicyService.cs`

## Implemented Discovery Contract

The current public-discovery surface is website- and Blog-focused. Event URLs and Event structured
data are removed.

Implemented today:

- public `robots.txt` behavior;
- public `sitemap.xml` with approved static routes and published Blog articles;
- page titles, descriptions, and canonical links on the Blog index and article pages;
- noindex metadata on authenticated Blog management/editor pages; and
- request-aware absolute sitemap URLs through `IPublicUrlBuilder`.

## Sitemap

`GET /sitemap.xml` publishes these static routes:

| Route | Change frequency | Priority |
|---|---:|---:|
| `/` | daily | 1.0 |
| `/welcome` | weekly | 0.8 |
| `/blog` | weekly | 0.8 |
| `/about` | monthly | 0.5 |
| `/contact` | monthly | 0.5 |
| `/community-guidelines` | yearly | 0.3 |

It also pages through published Blog summaries and emits `/blog/{escaped-slug}` only when the slug
is non-empty and `PublishedAtUtc` is not in the future. Duplicate paths are removed. Blog article
entries use weekly frequency and priority 0.7.

The endpoint is tenant-aware and output-cached. It does not claim every application route is
indexable.

## Robots

The Blazor host serves `GET /robots.txt`:

| Environment | Behavior |
|---|---|
| Production | Allows crawling and advertises the current host's `/sitemap.xml` |
| Other environments | `Disallow: /` |

The generated sitemap URL respects forwarded host/protocol processing. Non-production environments
must not be described as indexable.

## Blog Metadata

### `/blog`

- title: `Blog | ISLAMU`
- canonical: `/blog` resolved against the current public origin
- description: an ISLAMU editorial/community summary

### `/blog/{slug}`

- title derived from the loaded article title;
- canonical derived from the requested escaped slug; and
- description derived from the article title, with a safe generic fallback.

The public article renderer rejects unpublished/future content and renders sanitized Markdown.
Authenticated `/blog/manage*` pages emit `noindex,nofollow`.

## Navigation And Discoverability

The public `SiteMegaMenu` links directly to Home, Blog, About, Contact, and Community Guidelines.
These are implemented destinations, not placeholders. Dynamic catalog/product links are not part
of the navigation or SEO contract. Privacy and Terms remain deferred and absent from navigation and
the sitemap until approved legal text and accountable ownership exist.

## Render Policy Boundary

`RuntimeRenderPolicyService` currently classifies only `/`, `/welcome`, and `/home` as `PublicSeo`.
Blog routes fall into `Operational`, while the Blog components explicitly use
`InteractiveServer`. The service always returns `InteractiveServer`; route-group settings influence
prerender only.

Therefore, do not claim Blog prerendering from the `SeoBalanced` preset. If Blog routes need
`PublicSeo` prerender behavior, that requires a source change and its own verification.

## Not Implemented

- Blog `Article` JSON-LD or other Blog structured data.
- `hreflang`/alternate-language metadata for Blog content.
- Site-wide dynamic metadata for every route.
- Search Console integration, keyword tooling, SEO scoring, or automated audits.
- A standalone SEO administration page.
- Public Privacy or Terms pages and their sitemap entries.
- A generated checked-in website OpenAPI/client contract; that is an API generation concern, not
  SEO evidence.

Do not publish these as available capabilities until source and observable verification exist.

## Content Governance

- Only published, owner-approved pages enter navigation and the sitemap.
- Titles/descriptions must be factual and localized with the page content.
- A canonical URL must represent the public route actually served.
- Draft/editor/admin pages remain non-indexable.
- Analytics and email tracking require their own consent basis; SEO is not a reason to bypass it.

## Related

- [PROJECT.md](PROJECT.md)
- [CONTENT_ROUTE_INVENTORY.md](CONTENT_ROUTE_INVENTORY.md)
- [RENDER_POLICIES.md](RENDER_POLICIES.md)
- [ACCESSIBILITY.md](ACCESSIBILITY.md)
- [LOCALIZATION.md](LOCALIZATION.md)
