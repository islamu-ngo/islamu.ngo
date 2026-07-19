// ABOUTME: HATEOAS link policies for Blog detail resources and collection items.
// ABOUTME: Emits public reading links and permission-gated editorial affordances.

namespace Explore.API.Hateoas.Policies;

using System.Security.Claims;
using Explore.Application.Authorization;
using Explore.Application.Contracts.Hateoas;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Blog;
using Explore.Application.Hateoas;

public sealed class BlogDetailLinkPolicy : ILinkPolicy<BlogDetailDto>
{
    public IEnumerable<LinkDefinition> GetLinks(BlogDetailDto dto, ClaimsPrincipal? user)
    {
        yield return new LinkDefinition(
            LinkRelations.Self,
            RouteNames.GetPublishedBlogBySlug,
            new { slug = dto.Slug },
            "GET",
            dto.Title,
            Condition: () => dto.PublishedAtUtc is not null);

        yield return new LinkDefinition(
            LinkRelations.Collection,
            RouteNames.GetPublishedBlogs,
            null,
            "GET",
            "Published blog posts");

        yield return new LinkDefinition(
            "management-detail",
            RouteNames.GetBlogByIdForManagement,
            new { id = dto.Id },
            "GET",
            "Manage blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.View, ResourceDescriptors.BlogDetail, dto);

        yield return new LinkDefinition(
            LinkRelations.Edit,
            RouteNames.UpdateBlog,
            new { id = dto.Id },
            "PATCH",
            "Update blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.Update, ResourceDescriptors.BlogDetail, dto);

        yield return new LinkDefinition(
            LinkRelations.Delete,
            RouteNames.DeleteBlog,
            new { id = dto.Id },
            "DELETE",
            "Delete blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.Delete, ResourceDescriptors.BlogDetail, dto);

        yield return new LinkDefinition(
            LinkRelations.Publish,
            RouteNames.PublishBlog,
            new { id = dto.Id },
            "POST",
            "Publish blog post",
            RequiresAuth: true,
            Condition: () => dto.PublishedAtUtc is null)
            .RequirePermission(AuthorizationActions.Blogs.Publish, ResourceDescriptors.BlogDetail, dto);

        yield return new LinkDefinition(
            "unpublish",
            RouteNames.UnpublishBlog,
            new { id = dto.Id },
            "POST",
            "Unpublish blog post",
            RequiresAuth: true,
            Condition: () => dto.PublishedAtUtc is not null)
            .RequirePermission(AuthorizationActions.Blogs.Unpublish, ResourceDescriptors.BlogDetail, dto);
    }
}

public sealed class BlogCollectionLinkPolicy(ITenantContext tenantContext) : ICollectionLinkPolicy<BlogSummaryDto>
{
    public IEnumerable<LinkDefinition> GetItemLinks(BlogSummaryDto dto, ClaimsPrincipal? user)
    {
        yield return new LinkDefinition(
            LinkRelations.Self,
            RouteNames.GetPublishedBlogBySlug,
            new { slug = dto.Slug },
            "GET",
            dto.Title,
            Condition: () => dto.PublishedAtUtc is not null);

        yield return new LinkDefinition(
            "management-detail",
            RouteNames.GetBlogByIdForManagement,
            new { id = dto.Id },
            "GET",
            "Manage blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.View, ResourceDescriptors.BlogSummary, dto);

        yield return new LinkDefinition(
            LinkRelations.Edit,
            RouteNames.UpdateBlog,
            new { id = dto.Id },
            "PATCH",
            "Update blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.Update, ResourceDescriptors.BlogSummary, dto);

        yield return new LinkDefinition(
            LinkRelations.Delete,
            RouteNames.DeleteBlog,
            new { id = dto.Id },
            "DELETE",
            "Delete blog post",
            RequiresAuth: true)
            .RequirePermission(AuthorizationActions.Blogs.Delete, ResourceDescriptors.BlogSummary, dto);

        yield return new LinkDefinition(
            LinkRelations.Publish,
            RouteNames.PublishBlog,
            new { id = dto.Id },
            "POST",
            "Publish blog post",
            RequiresAuth: true,
            Condition: () => dto.PublishedAtUtc is null)
            .RequirePermission(AuthorizationActions.Blogs.Publish, ResourceDescriptors.BlogSummary, dto);

        yield return new LinkDefinition(
            "unpublish",
            RouteNames.UnpublishBlog,
            new { id = dto.Id },
            "POST",
            "Unpublish blog post",
            RequiresAuth: true,
            Condition: () => dto.PublishedAtUtc is not null)
            .RequirePermission(AuthorizationActions.Blogs.Unpublish, ResourceDescriptors.BlogSummary, dto);
    }

    public IEnumerable<LinkDefinition> GetCollectionLinks(ClaimsPrincipal? user)
    {
        yield return new LinkDefinition(
            LinkRelations.Create,
            RouteNames.CreateBlog,
            null,
            "POST",
            "Create blog post",
            RequiresAuth: true)
            .RequirePermission(
                AuthorizationActions.Blogs.Create,
                ResourceKinds.Blog,
                tenantContext.TenantId.ToString(),
                new Dictionary<string, object> { ["tenantId"] = tenantContext.TenantId.ToString() },
                new AuthorizationScope(TenantId: tenantContext.TenantId.ToString()));
    }
}
