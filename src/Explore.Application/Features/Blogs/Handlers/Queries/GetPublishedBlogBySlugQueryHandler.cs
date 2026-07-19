// ABOUTME: Handles public Blog detail lookup by tenant and normalized slug.
// ABOUTME: Fails closed by returning null unless the post is published at or before current UTC time.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Services;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Queries;

public sealed class GetPublishedBlogBySlugQueryHandler(IBlogRepository blogRepository)
    : IRequestHandler<GetPublishedBlogBySlugQuery, BlogDetailDto?>
{
    public async Task<BlogDetailDto?> Handle(
        GetPublishedBlogBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var validator = new GetPublishedBlogBySlugQueryValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        string slug = SlugGenerator.FromTitle(request.Slug);
        if (slug.Length == 0)
        {
            return null;
        }

        var blog = await blogRepository.GetByTenantAndSlugAsync(request.TenantId, slug, cancellationToken);
        if (blog?.PublishedAtUtc is not DateTime publishedAtUtc || publishedAtUtc > DateTime.UtcNow || blog.IsDeleted)
        {
            return null;
        }

        return BlogDtoMapper.ToDetail(blog);
    }
}
