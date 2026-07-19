// ABOUTME: Handles Blog updates with manual validation, tenant-owned aggregate loading, and slug conflict checks.
// ABOUTME: Replaces category and tag joins through the aggregate before repository reconciliation.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Requests.Commands;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Domain;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Commands;

public sealed class UpdateBlogCommandHandler(
    IBlogRepository blogRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IStorageObjectRepository storageObjectRepository)
    : IRequestHandler<UpdateBlogCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        UpdateBlogCommand request,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateBlogCommandValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var blog = await blogRepository.GetByTenantAndIdAsync(request.TenantId, request.BlogId, cancellationToken);
        if (blog is null)
        {
            throw new NotFoundException(nameof(Blog), request.BlogId);
        }

        if (blog.ConcurrencyStamp != request.Blog.ExpectedConcurrencyStamp)
        {
            throw new ConcurrencyConflictException(
                ConcurrencyConflictException.ConcurrentUpdate,
                "The blog was modified by another request. Reload and retry.",
                nameof(Blog),
                request.BlogId.ToString());
        }

        string slug = BlogWriteHandlerSupport.NormalizeSlug(request.Blog.Slug, request.Blog.Title);
        if (slug.Length == 0)
        {
            throw BlogWriteHandlerSupport.ValidationFailure("Blog.Slug", "A non-empty slug is required.");
        }

        if (await blogRepository.SlugExistsAsync(request.TenantId, slug, request.BlogId, cancellationToken))
        {
            throw BlogWriteHandlerSupport.ValidationFailure("Blog.Slug", "A blog with this slug already exists.");
        }

        await BlogWriteHandlerSupport.ValidateCategoriesAsync(
            categoryRepository,
            request.TenantId,
            request.Blog.CategoryIds,
            cancellationToken);
        await BlogWriteHandlerSupport.ValidateTagsAsync(
            tagRepository,
            request.TenantId,
            request.Blog.TagIds,
            cancellationToken);
        await BlogWriteHandlerSupport.ValidateThumbnailAsync(
            storageObjectRepository,
            request.TenantId,
            request.Blog.ThumbnailStorageObjectId,
            cancellationToken);

        blog.Update(
            request.Blog.Title,
            slug,
            request.Blog.ContentMarkdown,
            request.Blog.ThumbnailStorageObjectId);
        blog.ReplaceCategories(request.Blog.CategoryIds);
        blog.ReplaceTags(request.Blog.TagIds);

        var updated = await blogRepository.UpdateAsync(blog, cancellationToken);

        return new BaseCommandResponse<Guid>
        {
            Id = updated.Id,
            Success = true,
            Message = "Blog updated successfully."
        };
    }
}
