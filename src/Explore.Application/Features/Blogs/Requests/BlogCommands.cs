// ABOUTME: Defines tenant-scoped Blog write requests and their authorization metadata.
// ABOUTME: Keeps Blog commands in the Commands namespace required by the CQRS architecture.

using Explore.Application.Authorization;
using Explore.Application.DTOs.Blog;
using Explore.Application.Responses;
using Explore.Application.Features.Blogs.Requests;
using MediatR;

namespace Explore.Application.Features.Blogs.Requests.Commands;

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
