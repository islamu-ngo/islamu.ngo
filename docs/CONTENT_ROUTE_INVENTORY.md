<!-- ABOUTME: Source-backed truth inventory for public ISLAMU content, routes, navigation, calls to action, locales, analytics, and email capture. -->
<!-- ABOUTME: Separates implemented behavior from planned or deferred publication without inventing owner, legal, or impact claims. -->

# Content And Route Truth Inventory

> **Audience:** Product/content owners | Contributors | First-party operators | AI agents
> **Status:** Active
> **Owner:** Product | Content | Frontend
> **Last Verified:** 2026-07-19
> **Source Anchors:** `src/Explore.Blazor.Client/Routes.razor`, `src/Explore.Blazor.Client/Layout/SiteMegaMenu.razor.cs`, `src/Explore.API/Controllers/SitemapController.cs`, `src/Explore.Blazor.Client/Shared/AnalyticsInitializer.razor`, `src/Explore.Infrastructure/Localization/Bundles/`

This inventory governs statements about the current first-party website. `Implemented` means the
named source surface exists; it does not imply owner approval of every tenant-supplied word or a
completed launch audit. `Planned` means the direction is approved but no complete public journey is
available. `Deferred` means publication is withheld until ownership, content, consent, or legal
evidence exists. Planned and Deferred items must not become placeholder links.

## Public Claims

| Claim or content family | State | Accountable owner | Publication boundary | Evidence |
|---|---|---|---|---|
| ISLAMU operates one first-party digital front door | Implemented | Product | May describe the website and repository; does not imply white-label or supported self-hosting | `docs/PROJECT.md`; `README.md` |
| Public editorial Blog with published article discovery | Implemented | Editorial | May describe `/blog` and published articles; draft/admin behavior is not public content | Blog pages and `BlogController` |
| About, Contact, and Community Guidelines information | Implemented | Product / Content | Routes exist. Contact has no public form or published address; guidelines content is operator-configured and can be unavailable | Public page components; `Routes.razor` |
| English, French, and Arabic/RTL public-content parity | Planned | Content / Localization | Culture selection and fallback bundles exist, but public page copy is predominantly hard-coded English | `CultureRegistry`; starter bundles; public Razor pages |
| Public Privacy and Terms policies | Deferred | Legal / Product | No route, menu item, footer seed, or sitemap entry until approved text and ownership exist | `docs/PRIVACY_DATA_INVENTORY.md`; absence tests |
| Newsletter, waitlist, campaigns, donations, or marketing email engagement | Deferred | Product / Legal / Operations | No public collection form, preference journey, or CTA | Public routes and pages |
| Impact statistics, testimonials, campaign outcomes, imagery rights, or new program promises | Deferred | Product / Content | Repository source does not prove these claims | Non-code refactor workstream |

## Routes And Discovery

| Route or route family | State | Audience need | Navigation / indexing | Owner | Locale readiness | Evidence |
|---|---|---|---|---|---|---|
| `/`, `/home`, `/welcome` | Implemented | Understand the site or enter an existing account | Home is in the menu; all three are in the route table; `/` and `/welcome` are in the sitemap | Product / Frontend | English page copy; locale infrastructure only | `Routes.razor`; `HomeStart.razor`; `SitemapController` |
| `/blog`, `/blog/{slug}` | Implemented | Find and read published ISLAMU content | Blog is in the menu and sitemap; published articles are added dynamically | Editorial / Frontend | English UI; no alternate-language article model or `hreflang` | Blog pages; `SitemapController` |
| `/about`, `/contact`, `/community-guidelines` | Implemented | Understand ISLAMU, find contact status, or read configured participation guidance | All three are in the menu and sitemap | Product / Content | English UI; guidelines depend on operator-supplied content | Page components; `SiteMegaMenu`; `SitemapController` |
| `/organization/profile/{id}`, `/organization/reviews/{id}`, `/group/profile/{id}` | Implemented | View retained community actor surfaces | Not advertised in the public menu or sitemap | Community / Platform | English UI | `Routes.razor` |
| `/login`, `/logout` | Implemented | Enter or leave an existing account | Login is a welcome-page CTA; neither route is indexed | Identity / Frontend | English UI | `Routes.razor`; `LandingPageForNonUsers.razor` |
| `/startup`, `/setup`, `/onboarding/auth-provider`, `/onboarding/authz-provider`, `/errors/*` | Implemented | Operate setup or recover from an error | Operational only; absent from public navigation and sitemap | Platform / Operations | English UI | `Routes.razor` |
| `/blog/manage`, `/blog/manage/new`, `/blog/manage/{id}` | Implemented | Create and manage Blog content | Authenticated and `noindex`; absent from public navigation | Editorial | English UI | Blog management/editor components |
| `/robots.txt`, `/sitemap.xml` | Implemented | Support crawler discovery | Machine endpoints; sitemap contains only approved static routes and published Blog articles | Frontend / SEO | Locale-neutral | `RobotsController`; `SitemapController` |
| `/privacy`, `/terms` | Deferred | Read approved public legal policies | No routes, menu items, footer seeds, or sitemap entries | Legal / Product | Not applicable until approved source text exists | Route/menu/sitemap absence tests |

Authenticated user, tenant, and instance administration routes remain technical application
surfaces. Their presence is not a public product claim or a public call to action.

## Navigation And Calls To Action

| Navigation or action | State | Destination / effect | Owner | Truth condition |
|---|---|---|---|---|
| Static `SiteMegaMenu` groups: Discover, About ISLAMU, Community | Implemented | Home, Blog, About, Contact, Community Guidelines | Product / Frontend | Every item resolves to an implemented route |
| Welcome-page account entry | Implemented | `/login` | Identity / Frontend | Sign-in only; the UI does not promise self-service account creation |
| Blog article links, search, pagination, and article back link | Implemented | Published Blog queries and `/blog/{slug}` | Editorial / Frontend | Only published/current articles are public |
| About-to-Contact and Contact-to-Guidelines links | Implemented | `/contact`, `/community-guidelines` | Product / Content | These are route links, not a contact submission or email-capture promise |
| Operator-configured home content blocks, CTAs, organization website, and footer links | Implemented | Configuration-dependent URLs | First-party content operator | No specific destination is promised until configured, reviewed, and observable |
| Privacy, Terms, newsletter, waitlist, donation, campaign, and preference-center CTAs | Deferred | None | Legal / Product / Operations | Must remain absent until their complete journeys are approved and implemented |

## Locale Readiness

| Locale | State | Current honest parity | Owner | Launch gap |
|---|---|---|---|---|
| English (`en`) | Implemented | Starter fallback keys: app name and loading label; current public page copy is English | Content / Localization | Editorial review still applies |
| French (`fr`) | Implemented | Same starter fallback keys and first-party `ISLAMU` app name | Localization | Public page strings are not comprehensively keyed or translated |
| Arabic (`ar`, RTL) | Implemented | Same starter fallback keys and first-party `ISLAMU` app name; culture registry marks RTL | Localization / Accessibility | Public page strings and full RTL journey still require manual review |
| Complete three-locale public journey | Planned | Not yet achieved | Content / Localization / Accessibility | Key extraction, translation ownership, content review, and three-locale browser walk-through |

Starter-bundle key parity protects offline fallback behavior only. It must not be used as evidence
that marketing, Blog, navigation, consent, errors, or authenticated journeys are fully localized.

## Analytics And Email Capture

| Data interaction | State | Owner | Consent / data boundary | Evidence |
|---|---|---|---|---|
| Route page-view analytics | Implemented | Platform / Operations | Runs only through configured governance and the consent state machine; tracks normalized path plus bounded navigation context | `AnalyticsInitializer.razor`; `analytics-bridge.js` |
| Optional provider autocapture | Implemented | Platform / Privacy | Disabled by default and configuration-dependent; consent/cookieless policy applies and session replay/heatmaps are disabled in pre-consent cookieless mode | Analytics governance components and bootstrap model |
| Named campaign, CTA, conversion, or content analytics events | Deferred | Product / Analytics / Privacy | No public caller currently emits a named custom event | `IAnalyticsInterop`; client call-site search |
| Public newsletter, waitlist, donation, or marketing email capture | Deferred | Product / Legal / Operations | No form, storage contract, consent copy, withdrawal path, or public CTA exists | Public route/page search |
| Transactional email, invitations, notification preferences, and unsubscribe infrastructure | Implemented | Platform / Operations | Authenticated or transactional behavior; not evidence of public marketing capture | Email/outbox and notification contracts |

## Change Rule

Any change that adds a public claim, route, navigation item, CTA, locale promise, analytics event, or
email capture must update this inventory in the same change. It must name an owner, evidence source,
consent/legal dependency where relevant, and one of the three truth states before publication.

## Related

- [Product definition](PROJECT.md)
- [SEO and public discovery](SEO.md)
- [Localization](LOCALIZATION.md)
- [Privacy data inventory](PRIVACY_DATA_INVENTORY.md)
