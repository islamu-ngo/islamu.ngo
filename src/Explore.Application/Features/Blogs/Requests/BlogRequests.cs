// ABOUTME: MediatR request contracts for Blog public reads and tenant-scoped editorial writes.
// ABOUTME: Carries tenant and aggregate identifiers explicitly so future handlers can fail closed.

using Explore.Application.Authorization;
using Explore.Application.DTOs.Blog;
using Explore.Application.Responses;
using MediatR;

namespace Explore.Application.Features.Blogs.Requests;

public sealed record GetPublishedBlogsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    Guid? TagId = null,
    string? Search = null) : IRequest<PaginatedResult<BlogSummaryDto>>;

public sealed record GetPublishedBlogBySlugQuery(
    Guid TenantId,
    string Slug) : IRequest<BlogDetailDto?>;

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.View)]
public sealed record GetBlogsForManagementQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedResult<BlogSummaryDto>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => TenantId == Guid.Empty ? null : TenantId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => TenantId == Guid.Empty
        ? null
        : new Dictionary<string, object>
        {
            ["tenantId"] = TenantId.ToString("D")
        };
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.View)]
public sealed record GetBlogByIdForManagementQuery(
    Guid TenantId,
    Guid BlogId) : IRequest<BlogDetailDto?>, ISecureRequest
{
    string? ISecureRequest.ResourceId => BlogId == Guid.Empty ? null : BlogId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => BlogRequestAuthorizationContext.Attributes(TenantId, BlogId);
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.Create)]
public sealed record CreateBlogCommand(
    Guid TenantId,
    CreateBlogDto Blog) : IRequest<BaseCommandResponse<Guid>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => TenantId == Guid.Empty ? null : TenantId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => TenantId == Guid.Empty
        ? null
        : new Dictionary<string, object>
        {
            ["tenantId"] = TenantId.ToString("D")
        };
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.Update)]
public sealed record UpdateBlogCommand(
    Guid TenantId,
    Guid BlogId,
    UpdateBlogDto Blog) : IRequest<BaseCommandResponse<Guid>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => BlogId == Guid.Empty ? null : BlogId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => BlogRequestAuthorizationContext.Attributes(TenantId, BlogId);
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.Publish)]
public sealed record PublishBlogCommand(
    Guid TenantId,
    Guid BlogId) : IRequest<BaseCommandResponse<Guid>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => BlogId == Guid.Empty ? null : BlogId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => BlogRequestAuthorizationContext.Attributes(
        TenantId,
        BlogId,
        AuthorizationActions.Blogs.Publish);
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.Unpublish)]
public sealed record UnpublishBlogCommand(
    Guid TenantId,
    Guid BlogId) : IRequest<BaseCommandResponse<Guid>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => BlogId == Guid.Empty ? null : BlogId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => BlogRequestAuthorizationContext.Attributes(
        TenantId,
        BlogId,
        AuthorizationActions.Blogs.Unpublish);
}

[AuthorizeResource(ResourceKinds.Blog, AuthorizationActions.Blogs.Delete)]
public sealed record DeleteBlogCommand(
    Guid TenantId,
    Guid BlogId) : IRequest<BaseCommandResponse<Guid>>, ISecureRequest
{
    string? ISecureRequest.ResourceId => BlogId == Guid.Empty ? null : BlogId.ToString("D");

    IDictionary<string, object>? ISecureRequest.ResourceAttributes => BlogRequestAuthorizationContext.Attributes(TenantId, BlogId);
}

file static class BlogRequestAuthorizationContext
{
    public static IDictionary<string, object>? Attributes(Guid tenantId, Guid blogId, string? publicationIntent = null)
    {
        if (tenantId == Guid.Empty || blogId == Guid.Empty)
        {
            return null;
        }

        var attributes = new Dictionary<string, object>
        {
            ["tenantId"] = tenantId.ToString("D"),
            ["blogId"] = blogId.ToString("D")
        };

        if (publicationIntent is not null)
        {
            attributes["publicationIntent"] = publicationIntent;
        }

        return attributes;
    }
}
