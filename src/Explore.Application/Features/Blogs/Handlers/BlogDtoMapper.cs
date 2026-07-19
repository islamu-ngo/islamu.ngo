// ABOUTME: Maps Blog domain entities to public Blog summary and detail DTOs inside Application.
// ABOUTME: Keeps repository reads entity-first while exposing only DTO-shaped CQRS responses.

using Explore.Application.DTOs.Blog;
using DomainBlog = Explore.Domain.Blog;

namespace Explore.Application.Features.Blogs.Handlers;

internal static class BlogDtoMapper
{
    public static BlogSummaryDto ToSummary(DomainBlog blog)
    {
        return new BlogSummaryDto
        {
            Id = blog.Id,
            TenantId = blog.TenantId,
            Title = blog.Title,
            Slug = blog.Slug,
            ThumbnailStorageObjectId = blog.ThumbnailStorageObjectId,
            PublishedAtUtc = blog.PublishedAtUtc,
            CategoryIds = blog.CategoryLinks.Select(link => link.CategoryId).ToList(),
            TagIds = blog.TagLinks.Select(link => link.TagId).ToList()
        };
    }

    public static BlogDetailDto ToDetail(DomainBlog blog)
    {
        return new BlogDetailDto
        {
            Id = blog.Id,
            TenantId = blog.TenantId,
            Title = blog.Title,
            Slug = blog.Slug,
            ThumbnailStorageObjectId = blog.ThumbnailStorageObjectId,
            PublishedAtUtc = blog.PublishedAtUtc,
            CategoryIds = blog.CategoryLinks.Select(link => link.CategoryId).ToList(),
            TagIds = blog.TagLinks.Select(link => link.TagId).ToList(),
            ContentMarkdown = blog.ContentMarkdown,
            ConcurrencyStamp = blog.ConcurrencyStamp
        };
    }
}
