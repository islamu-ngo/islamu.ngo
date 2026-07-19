// ABOUTME: Handles Blog creation with manual validation, slug normalization, and related entity checks.
// ABOUTME: Persists the Blog aggregate after replacing category and tag joins through domain methods.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Application.Services;
using Explore.Domain;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Commands;

public sealed class CreateBlogCommandHandler(
    IBlogRepository blogRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IStorageObjectRepository storageObjectRepository)
    : IRequestHandler<CreateBlogCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        CreateBlogCommand request,
        CancellationToken cancellationToken)
    {
        var validator = new CreateBlogCommandValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        string slug = BlogWriteHandlerSupport.NormalizeSlug(request.Blog.Slug, request.Blog.Title);
        if (slug.Length == 0)
        {
            throw BlogWriteHandlerSupport.ValidationFailure("Blog.Slug", "A non-empty slug is required.");
        }

        if (await blogRepository.SlugExistsAsync(request.TenantId, slug, null, cancellationToken))
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

        var blog = Blog.Create(
            request.TenantId,
            request.Blog.Title,
            slug,
            request.Blog.ContentMarkdown,
            request.Blog.ThumbnailStorageObjectId);

        blog.ReplaceCategories(request.Blog.CategoryIds);
        blog.ReplaceTags(request.Blog.TagIds);

        var created = await blogRepository.CreateAsync(blog, cancellationToken);

        return new BaseCommandResponse<Guid>
        {
            Id = created.Id,
            Success = true,
            Message = "Blog created successfully."
        };
    }
}

internal static class BlogWriteHandlerSupport
{
    public static string NormalizeSlug(string? suppliedSlug, string title)
    {
        return SlugGenerator.FromTitle(string.IsNullOrWhiteSpace(suppliedSlug) ? title : suppliedSlug);
    }

    public static async Task ValidateCategoriesAsync(
        ICategoryRepository categoryRepository,
        Guid tenantId,
        IReadOnlyCollection<Guid> categoryIds,
        CancellationToken cancellationToken)
    {
        foreach (Guid categoryId in categoryIds.Distinct())
        {
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);

            if (category is null || category.TenantId != tenantId)
            {
                throw new NotFoundException(nameof(Category), categoryId);
            }
        }
    }

    public static async Task ValidateTagsAsync(
        ITagRepository tagRepository,
        Guid tenantId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        foreach (Guid tagId in tagIds.Distinct())
        {
            var tag = await tagRepository.GetByIdAsync(tagId, cancellationToken);

            if (tag is null || tag.TenantId != tenantId)
            {
                throw new NotFoundException(nameof(Tag), tagId);
            }
        }
    }

    public static async Task ValidateThumbnailAsync(
        IStorageObjectRepository storageObjectRepository,
        Guid tenantId,
        Guid? thumbnailStorageObjectId,
        CancellationToken cancellationToken)
    {
        if (!thumbnailStorageObjectId.HasValue)
        {
            return;
        }

        var thumbnail = await storageObjectRepository.GetByIdAsync(thumbnailStorageObjectId.Value, cancellationToken);

        if (thumbnail is null || thumbnail.TenantId != tenantId || thumbnail.Purpose != StorageObjectPurposes.BlogImage)
        {
            throw ValidationFailure(
                "Blog.ThumbnailStorageObjectId",
                "Thumbnail storage object must belong to the tenant and have the blog_image purpose.");
        }
    }

    public static FluentValidation.ValidationException ValidationFailure(string propertyName, string message)
    {
        return new FluentValidation.ValidationException([new ValidationFailure(propertyName, message)]);
    }
}
