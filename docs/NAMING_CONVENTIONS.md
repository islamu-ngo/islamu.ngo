<!-- ABOUTME: Naming conventions for the current first-party ISLAMU website source tree. -->
<!-- ABOUTME: Uses Blog examples while distinguishing legacy project identifiers from removed Event product names. -->

# Naming Conventions

> **Audience:** Contributors | Maintainers | AI agents
> **Status:** Active
> **Owner:** Architecture
> **Last Verified:** 2026-07-18
> **Source Anchors:** `Explore.slnx`, `src/Explore.Application/Features/Blogs/`, `src/Explore.API/Controllers/BlogController.cs`, `tests/Event.Architecture.Tests/NamingConventionTests.cs`, `src/Explore.Blazor.Client/nswag.json`

## Repository Identity

- Product name: **ISLAMU website** or **ISLAMU web application**.
- Main source projects keep the `Explore.*` prefix.
- Several test/runtime support projects keep the `Event.*` prefix.
- These prefixes are source identifiers, not public branding and not proof of a live Event product.
- Project/namespace renaming is a separate all-at-once workstream; do not rename files piecemeal.

## General C# Rules

| Element | Convention | Example |
|---|---|---|
| Type/public member | PascalCase | `BlogRepository`, `PublishedAtUtc` |
| Local/parameter | camelCase | `blogId`, `cancellationToken` |
| Private field | `_camelCase` | `_blogRepository` |
| Interface | `I` + PascalCase | `IBlogRepository` |
| Async method | `Async` suffix | `GetPublishedBlogsAsync` |
| Constant | PascalCase | `DefaultPageSize` |
| Namespace | file-scoped | `namespace Explore.Application.Features.Blogs;` |

Every source file starts with two ABOUTME lines according to `AGENTS.md`.

## File And Type Patterns

| Concern | Pattern | Current example |
|---|---|---|
| Aggregate/entity | `{Entity}.cs` | `Blog.cs`, `Organization.cs` |
| Repository contract | `I{Entity}Repository.cs` | `IBlogRepository.cs` |
| Repository implementation | `{Entity}Repository.cs` | `BlogRepository.cs` |
| EF configuration | `{Entity}Configuration.cs` | `BlogConfiguration.cs` |
| Controller | `{Entity}Controller.cs` | `BlogController.cs` |
| DTO family | `{Entity}*Dto` | `BlogSummaryDto`, `BlogDetailDto` |
| Command | `{Verb}{Entity}Command` | `CreateBlogCommand`, `PublishBlogCommand` |
| Command handler | `{Verb}{Entity}CommandHandler` | `PublishBlogCommandHandler` |
| Query | descriptive `{Verb}{Subject}Query` | `GetPublishedBlogsQuery` |
| Query handler | `{Query}Handler` | `GetPublishedBlogsQueryHandler` |
| Specification | `{Entity}QuerySpecification` | `BlogQuerySpecification` |
| Blazor page | descriptive PascalCase | `BlogIndex.razor`, `BlogEditor.razor` |
| Scoped CSS | component filename + `.css` | `SiteMegaMenu.razor.css` |
| Client service | `{Concern}Service` | `BlogService` |

Follow the feature's existing local pattern rather than forcing a repository-wide rename during an
unrelated change.

## CQRS And Folder Names

Feature folders are plural PascalCase, for example `Features/Blogs/`. Blog uses:

```text
Features/Blogs/
├── Requests/BlogRequests.cs
├── Validators/BlogValidators.cs
└── Handlers/
    ├── Commands/*CommandHandler.cs
    └── Queries/*QueryHandler.cs
```

Commands describe writes; queries describe reads. Validators are manually instantiated by the
owning handler/service. Repositories return entities and handlers map to DTOs.

## DTO Names

Use a name that communicates payload purpose:

| Purpose | Pattern | Blog example |
|---|---|---|
| Collection read | `{Entity}SummaryDto` or existing `{Entity}ListDto` | `BlogSummaryDto` |
| Detail read | `{Entity}DetailDto` or existing `{Entity}Dto` | `BlogDetailDto` |
| Create input | `Create{Entity}Dto` | `CreateBlogDto` |
| Update input | `Update{Entity}Dto` | `UpdateBlogDto` |

Do not introduce a second DTO suffix for the same semantic role merely for consistency. Normalize
new APIs at the owning feature boundary.

## Controller Routes And Names

Controllers use `[Route("api/[controller]")]` unless a source-backed integration requires a
different stable route. `BlogController` therefore serves `/api/blog`.

Each action needs:

- explicit HTTP verb and route template;
- stable `RouteNames` constant;
- endpoint classification;
- response types; and
- HAL policy coverage when it exposes resource affordances.

Use descriptive route names such as `GetPublishedBlogs`, `GetPublishedBlogBySlug`, `CreateBlog`,
`PublishBlog`, and `UnpublishBlog`.

## Blazor Routes And Components

- Public content uses readable lowercase routes: `/blog`, `/blog/:Slug`.
- Authenticated editorial routes stay under `/blog/manage`.
- Components/types remain PascalCase: `BlogIndex`, `BlogArticle`, `SiteMegaMenu`.
- CSS uses BEM blocks based on the component: `.site-mega-menu__panel`,
  `.blog-article__body`.

## Persistence Names

- Aggregate IDs use `Guid`; lookup IDs use the repository's current lookup type.
- C# properties use PascalCase; EF/database naming conventions produce snake_case database names.
- Link entities name both sides, for example `BlogCategory` and `BlogTag`.
- Tenant-scoped entities expose `TenantId`; audit/soft-delete fields use the established interfaces.

## Test Names

Test projects retain their current names from `Explore.slnx`, including `Event.*`. Test classes name
the subject, for example `BlogCommandHandlerTests`, `BlogRepositoryTests`, and
`SiteMegaMenuTests`.

Test method/display names should state the behavior and expected outcome. Do not rename test
projects solely to remove the legacy prefix.

## OpenAPI And Generated Client Names

The configured canonical identities are:

- schema target: `schemas/openapi_islamu.json`;
- client interface/class: `IIslamuApiClient` / `IslamuApiClient`; and
- generated output target: `Clients/IslamuApiClient.g.cs`.

The schema and generated client were absent on 2026-07-18. Treat these as configured names, not
existing artifacts. The removed `openapi_islamu-event.json` and `EventApiClient.g.cs` names must not
return.

## Public Writing

- Use **Blog** when naming the implemented product vertical because the current types/routes use
  that capitalization.
- Use **ISLAMU website** in public/product prose.
- Use **Event** only for a legacy project identifier, historical evidence, or generic .NET event,
  with enough context to prevent product confusion.
- Do not use self-host, white-label, or multi-tenant as public product positioning.

## Related

- [GOVERNANCE.md](GOVERNANCE.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [CODEBASE_STRUCTURE.md](CODEBASE_STRUCTURE.md)
- [TEMPLATE_GLOSSARY.md](TEMPLATE_GLOSSARY.md)
