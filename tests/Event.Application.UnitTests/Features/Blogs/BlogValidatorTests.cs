// ABOUTME: Unit tests for Blog query, command, and write-payload validators.
// ABOUTME: Covers tenant, pagination, slug, required Markdown, related IDs, and Arabic content validation.

using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Requests.Commands;
using Explore.Application.Features.Blogs.Validators;

namespace Event.Application.UnitTests.Features.Blogs;

public sealed class BlogValidatorTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid BlogId = Guid.NewGuid();

    [Test]
    public async Task Validate_WithEmptyTenantIds_ReturnsInvalid()
    {
        var listResult = await new GetPublishedBlogsQueryValidator().ValidateAsync(
            new GetPublishedBlogsQuery(Guid.Empty));
        var detailResult = await new GetPublishedBlogBySlugQueryValidator().ValidateAsync(
            new GetPublishedBlogBySlugQuery(Guid.Empty, "community-update"));
        var managementListResult = await new GetBlogsForManagementQueryValidator().ValidateAsync(
            new GetBlogsForManagementQuery(Guid.Empty));
        var managementDetailResult = await new GetBlogByIdForManagementQueryValidator().ValidateAsync(
            new GetBlogByIdForManagementQuery(Guid.Empty, BlogId));
        var createResult = await new CreateBlogCommandValidator().ValidateAsync(
            new CreateBlogCommand(Guid.Empty, CreateBlogDto()));
        var updateResult = await new UpdateBlogCommandValidator().ValidateAsync(
            new UpdateBlogCommand(Guid.Empty, BlogId, UpdateBlogDto()));

        await Assert.That(listResult.IsValid).IsFalse();
        await Assert.That(detailResult.IsValid).IsFalse();
        await Assert.That(managementListResult.IsValid).IsFalse();
        await Assert.That(managementDetailResult.IsValid).IsFalse();
        await Assert.That(createResult.IsValid).IsFalse();
        await Assert.That(updateResult.IsValid).IsFalse();
    }

    [Test]
    [Arguments(0, 20, "Page")]
    [Arguments(1, 0, "PageSize")]
    [Arguments(1, 101, "PageSize")]
    public async Task Validate_WithInvalidPageOrPageSize_ReturnsInvalid(int page, int pageSize, string propertyName)
    {
        var result = await new GetPublishedBlogsQueryValidator().ValidateAsync(
            new GetPublishedBlogsQuery(TenantId, page, pageSize));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == propertyName)).IsTrue();
    }

    [Test]
    [Arguments(0, 20, "Page")]
    [Arguments(1, 0, "PageSize")]
    [Arguments(1, 101, "PageSize")]
    public async Task Validate_ManagementListWithInvalidPageOrPageSize_ReturnsInvalid(int page, int pageSize, string propertyName)
    {
        var result = await new GetBlogsForManagementQueryValidator().ValidateAsync(
            new GetBlogsForManagementQuery(TenantId, page, pageSize));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == propertyName)).IsTrue();
    }

    [Test]
    public async Task Validate_ManagementDetailWithEmptyBlogId_ReturnsInvalid()
    {
        var result = await new GetBlogByIdForManagementQueryValidator().ValidateAsync(
            new GetBlogByIdForManagementQuery(TenantId, Guid.Empty));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "BlogId")).IsTrue();
    }

    [Test]
    public async Task Validate_WithEmptyDetailSlug_ReturnsInvalid()
    {
        var result = await new GetPublishedBlogBySlugQueryValidator().ValidateAsync(
            new GetPublishedBlogBySlugQuery(TenantId, string.Empty));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "Slug")).IsTrue();
    }

    [Test]
    [Arguments("", "## Body", "Title")]
    [Arguments("Community Update", "", "ContentMarkdown")]
    [Arguments("Community Update", "## Body", "Slug")]
    public async Task Validate_CreatePayloadWithRequiredFieldOrLongSlug_ReturnsInvalid(
        string title,
        string contentMarkdown,
        string propertyName)
    {
        var dto = CreateBlogDto(title, contentMarkdown, slug: propertyName == "Slug" ? new string('a', 201) : "community-update");

        var result = await new CreateBlogDtoValidator().ValidateAsync(dto);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == propertyName)).IsTrue();
    }

    [Test]
    public async Task Validate_CreatePayloadWithEmptyCategoryTagAndThumbnailIds_ReturnsInvalid()
    {
        var dto = CreateBlogDto(
            categoryIds: [Guid.Empty],
            tagIds: [Guid.Empty],
            thumbnailStorageObjectId: Guid.Empty);

        var result = await new CreateBlogDtoValidator().ValidateAsync(dto);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "CategoryIds[0]")).IsTrue();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "TagIds[0]")).IsTrue();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "ThumbnailStorageObjectId")).IsTrue();
    }

    [Test]
    public async Task Validate_UpdatePayloadWithEmptyCategoryTagAndThumbnailIds_ReturnsInvalid()
    {
        var dto = UpdateBlogDto(
            categoryIds: [Guid.Empty],
            tagIds: [Guid.Empty],
            thumbnailStorageObjectId: Guid.Empty);

        var result = await new UpdateBlogDtoValidator().ValidateAsync(dto);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "CategoryIds[0]")).IsTrue();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "TagIds[0]")).IsTrue();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "ThumbnailStorageObjectId")).IsTrue();
    }

    [Test]
    public async Task Validate_UpdatePayloadWithEmptyExpectedConcurrencyStamp_ReturnsInvalid()
    {
        var dto = UpdateBlogDto(expectedConcurrencyStamp: Guid.Empty);

        var result = await new UpdateBlogDtoValidator().ValidateAsync(dto);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(error => error.PropertyName == "ExpectedConcurrencyStamp")).IsTrue();
    }

    [Test]
    public async Task Validate_WithValidArabicTitleAndMarkdownPayload_ReturnsValid()
    {
        var result = await new CreateBlogDtoValidator().ValidateAsync(CreateBlogDto(
            title: "تحديث المجتمع الإسلامي",
            contentMarkdown: "## السلام عليكم\n\nهذا نص عربي صالح للنشر."));

        await Assert.That(result.IsValid).IsTrue();
    }

    private static CreateBlogDto CreateBlogDto(
        string title = "Community Update",
        string contentMarkdown = "## Body",
        string? slug = "community-update",
        IReadOnlyList<Guid>? categoryIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        Guid? thumbnailStorageObjectId = null) =>
        new()
        {
            Title = title,
            Slug = slug,
            ContentMarkdown = contentMarkdown,
            CategoryIds = categoryIds ?? [Guid.NewGuid()],
            TagIds = tagIds ?? [Guid.NewGuid()],
            ThumbnailStorageObjectId = thumbnailStorageObjectId
        };

    private static UpdateBlogDto UpdateBlogDto(
        IReadOnlyList<Guid>? categoryIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        Guid? thumbnailStorageObjectId = null,
        Guid? expectedConcurrencyStamp = null) =>
        new()
        {
            Title = "Community Update",
            Slug = "community-update",
            ContentMarkdown = "## Body",
            ExpectedConcurrencyStamp = expectedConcurrencyStamp ?? Guid.NewGuid(),
            CategoryIds = categoryIds ?? [Guid.NewGuid()],
            TagIds = tagIds ?? [Guid.NewGuid()],
            ThumbnailStorageObjectId = thumbnailStorageObjectId
        };
}
