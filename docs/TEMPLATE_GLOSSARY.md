<!-- ABOUTME: Placeholder glossary for repository templates and engineering examples. -->
<!-- ABOUTME: Uses the implemented Blog vertical as the project-specific substitution example. -->

# Template Glossary

> **Audience:** Contributors | Documentation authors | AI agents
> **Status:** Active
> **Owner:** Contributor Experience
> **Last Verified:** 2026-07-18
> **Source Anchors:** `docs/NAMING_CONVENTIONS.md`, `src/Explore.Application/Features/Blogs/`, `src/Explore.API/Controllers/BlogController.cs`

## Purpose

Templates use braces for values that must be replaced with source-backed names. Placeholder
examples are syntax demonstrations, not requirements to create every listed layer or abstraction.

## Core Placeholders

| Placeholder | Form | Blog substitution |
|---|---|---|
| `{Entity}` | PascalCase singular | `Blog` |
| `{Entities}` | PascalCase plural | `Blogs` |
| `{entity}` | camelCase singular | `blog` |
| `{entities}` | camelCase plural | `blogs` |
| `{entityId}` | camelCase identifier | `blogId` |
| `{Feature}` | PascalCase feature folder | `Blogs` |
| `{Route}` | lowercase public/API route | `blog` |
| `{Dto}` | concrete transport type | `BlogDetailDto` |
| `{Command}` | concrete write request | `PublishBlogCommand` |
| `{Query}` | concrete read request | `GetPublishedBlogsQuery` |

Pluralization is semantic. Do not mechanically append `s` when the source language requires a
different form.

## Project And Layer Placeholders

| Placeholder | Current value |
|---|---|
| `{Solution}` | `Explore.slnx` |
| `{DomainProject}` | `Explore.Domain` |
| `{ApplicationProject}` | `Explore.Application` |
| `{PersistenceProject}` | `Explore.Persistence` |
| `{InfrastructureProject}` | `Explore.Infrastructure` |
| `{ApiProject}` | `Explore.API` |
| `{BffProject}` | `Explore.Blazor` |
| `{ClientProject}` | `Explore.Blazor.Client` |
| `{AppHostProject}` | `Explore.AppHost` |

Legacy `Event.*` test/support project names are literal source identifiers, not placeholders for
the product name.

## Common Type Placeholders

| Placeholder | Meaning | Typical value |
|---|---|---|
| `{IdType}` | Aggregate identifier type | `Guid` |
| `{LookupIdType}` | Lookup identifier type | existing lookup type, usually `int` |
| `{ResponseType}` | Command response | `BaseCommandResponse<Guid>` |
| `{CollectionType}` | Paginated query response | `PaginatedResult<BlogSummaryDto>` |
| `{CancellationToken}` | Cooperative cancellation | `CancellationToken cancellationToken` |

Choose the type from the owning source contract; do not substitute a preferred type blindly.

## File Path Patterns

| Concern | Pattern | Blog example |
|---|---|---|
| Domain entity | `src/Explore.Domain/{Entity}.cs` | `src/Explore.Domain/Blog.cs` |
| CQRS feature | `src/Explore.Application/Features/{Feature}/` | `Features/Blogs/` |
| DTO | `src/Explore.Application/DTOs/{Entity}/` | `DTOs/Blog/` |
| Repository contract | `Contracts/Persistence/I{Entity}Repository.cs` | `IBlogRepository.cs` |
| Repository | `src/Explore.Persistence/Repositories/{Entity}Repository.cs` | `BlogRepository.cs` |
| EF configuration | `Configurations/Entities/{Entity}Configuration.cs` | `BlogConfiguration.cs` |
| Controller | `src/Explore.API/Controllers/{Entity}Controller.cs` | `BlogController.cs` |
| Blazor page area | `src/Explore.Blazor.Client/Pages/{Entities}/` | `Pages/Blogs/` |
| Blazor service | `Services/{Entities}/{Entity}Service.cs` | `Services/Blogs/BlogService.cs` |

## CQRS Substitutions

| Template | Blog example |
|---|---|
| `{Verb}{Entity}Command` | `CreateBlogCommand`, `PublishBlogCommand` |
| `{Verb}{Entity}CommandHandler` | `PublishBlogCommandHandler` |
| `Get{Entities}Query` | `GetPublishedBlogsQuery` |
| `Get{Entity}By{Key}Query` | `GetPublishedBlogBySlugQuery` |
| `{Query}Handler` | `GetPublishedBlogsQueryHandler` |
| `I{Entity}Repository` | `IBlogRepository` |
| `{Entity}QuerySpecification` | `BlogQuerySpecification` |

Templates must follow the feature's actual naming. The current Blog feature groups request records
in `BlogRequests.cs`; a template does not require one file per record.

## API And HAL Placeholders

| Placeholder | Blog value |
|---|---|
| `{Controller}` | `BlogController` |
| `{BaseRoute}` | `/api/blog` |
| `{RouteName}` | `GetPublishedBlogs` |
| `{ResourceKind}` | current Blog resource kind from `ResourceKinds` |
| `{Action}` | current Blog action from `AuthorizationActions.Blogs` |
| `{LinkRel}` | `edit`, `publish`, `unpublish`, `delete` |

HAL relation names are server contracts. Copy an existing relation only when it represents the same
semantics; never invent a client-only permission flag.

## Blazor Placeholders

| Placeholder | Blog/navigation value |
|---|---|
| `{PublicIndex}` | `BlogIndex` at `/blog` |
| `{PublicDetail}` | `BlogArticle` at `/blog/:Slug` |
| `{ManagementPage}` | `BlogManagement` at `/blog/manage` |
| `{EditorPage}` | `BlogEditor` |
| `{NavigationComponent}` | `SiteMegaMenu` |

Use implemented routes only. Planned and deferred destinations do not become template values for
public links.

## Substitution Example

Starting template:

```text
{Entity} = Blog
{Entities} = Blogs
{IdType} = Guid
{Controller} = BlogController
{BaseRoute} = /api/blog
{Repository} = IBlogRepository / BlogRepository
```

Resulting source flow:

```text
Blog page -> BlogService -> BlogController -> Blog CQRS handler -> BlogRepository -> Blog entity
```

Create only the pieces the feature actually needs. Reuse existing repository, CQRS, HAL, BFF, and
design-system patterns before adding abstractions.

## Rules For Template Authors

1. State which placeholders are required and which are optional.
2. Use one coherent source-backed example.
3. Keep product names and code identifiers distinct.
4. Do not preserve removed Event entities as examples.
5. Do not claim generated files exist merely because a template/config names an output.
6. Link to canonical conventions instead of duplicating long rule sets.

## Related

- [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md)
- [GOVERNANCE.md](GOVERNANCE.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
