// ABOUTME: Handles authenticated Blog management detail reads by tenant and aggregate ID.
// ABOUTME: Validates identifiers, routes cancellation to the repository, and maps Domain Blog entities to DTOs.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Validators;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Queries;

public sealed class GetBlogByIdForManagementQueryHandler(IBlogRepository blogRepository)
    : IRequestHandler<GetBlogByIdForManagementQuery, BlogDetailDto?>
{
    public async Task<BlogDetailDto?> Handle(
        GetBlogByIdForManagementQuery request,
        CancellationToken cancellationToken)
    {
        var validator = new GetBlogByIdForManagementQueryValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var blog = await blogRepository.GetByTenantAndIdAsync(request.TenantId, request.BlogId, cancellationToken);
        return blog is null ? null : BlogDtoMapper.ToDetail(blog);
    }
}
