// ABOUTME: Unit tests for Blog command handlers using repository substitutes and aggregate-state assertions.
// ABOUTME: Covers slug normalization, tenant-safe relations, lifecycle transitions, and cancellation routing.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Blog;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Handlers.Commands;
using Explore.Application.Features.Blogs.Requests.Commands;
using Explore.Domain;
using NSubstitute;

namespace Event.Application.UnitTests.Features.Blogs;

public sealed class BlogCommandHandlerTests
{
    [Test]
    public async Task Create_NormalizesTitleSlugAndDeduplicatesCategoryAndTagJoins()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var blogRepository = Substitute.For<IBlogRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var tagRepository = Substitute.For<ITagRepository>();
        var storageRepository = Substitute.For<IStorageObjectRepository>();
        Blog? created = null;
        categoryRepository.GetByIdAsync(categoryId, cancellationToken).Returns((Category?)Category(categoryId, tenantId));
        tagRepository.GetByIdAsync(tagId, cancellationToken).Returns((Tag?)Tag(tagId, tenantId));
        storageRepository.GetByIdAsync(thumbnailId, cancellationToken).Returns(StorageObject(thumbnailId, tenantId, StorageObjectPurposes.BlogImage));
        blogRepository.SlugExistsAsync(tenantId, "السلام-عليكم", null, cancellationToken).Returns(false);
        blogRepository.CreateAsync(Arg.Do<Blog>(blog => created = blog), cancellationToken)
            .Returns(call => Task.FromResult<Blog>(call.Arg<Blog>()!));
        var handler = new CreateBlogCommandHandler(blogRepository, categoryRepository, tagRepository, storageRepository);

        var result = await handler.Handle(
            new CreateBlogCommand(
                tenantId,
                CreateDto(
                    title: "  السلام عليكم  ",
                    slug: null,
                    thumbnailStorageObjectId: thumbnailId,
                    categoryIds: [categoryId, categoryId],
                    tagIds: [tagId, tagId])),
            cancellationToken);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Title).IsEqualTo("السلام عليكم");
        await Assert.That(created.Slug).IsEqualTo("السلام-عليكم");
        await Assert.That(created.ThumbnailStorageObjectId).IsEqualTo(thumbnailId);
        await Assert.That(created.CategoryLinks.Select(link => link.CategoryId).ToList()).IsEquivalentTo([categoryId]);
        await Assert.That(created.TagLinks.Select(link => link.TagId).ToList()).IsEquivalentTo([tagId]);
        await blogRepository.Received(1).SlugExistsAsync(tenantId, "السلام-عليكم", null, cancellationToken);
        await blogRepository.Received(1).CreateAsync(created, cancellationToken);
        await categoryRepository.Received(1).GetByIdAsync(categoryId, cancellationToken);
        await tagRepository.Received(1).GetByIdAsync(tagId, cancellationToken);
        await storageRepository.Received(1).GetByIdAsync(thumbnailId, cancellationToken);
    }

    [Test]
    public async Task Create_WithDuplicateSlug_FailsBeforeRelationLookups()
    {
        var tenantId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var blogRepository = Substitute.For<IBlogRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var tagRepository = Substitute.For<ITagRepository>();
        var storageRepository = Substitute.For<IStorageObjectRepository>();
        blogRepository.SlugExistsAsync(tenantId, "community-update", null, cancellationToken).Returns(true);
        var handler = new CreateBlogCommandHandler(blogRepository, categoryRepository, tagRepository, storageRepository);

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
            await handler.Handle(new CreateBlogCommand(tenantId, CreateDto()), cancellationToken));

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Errors.Any(error => error.PropertyName == "Blog.Slug")).IsTrue();
        await blogRepository.DidNotReceive().CreateAsync(Arg.Any<Blog>(), Arg.Any<CancellationToken>());
        await categoryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await tagRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await storageRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_WithCrossTenantCategoryOrTag_Fails()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var categoryFailure = TestContext(tenantId, cancellationToken);
        categoryFailure.BlogRepository.SlugExistsAsync(tenantId, "community-update", null, cancellationToken).Returns(false);
        categoryFailure.CategoryRepository.GetByIdAsync(categoryId, cancellationToken).Returns((Category?)Category(categoryId, otherTenantId));
        var tagFailure = TestContext(tenantId, cancellationToken);
        tagFailure.BlogRepository.SlugExistsAsync(tenantId, "community-update", null, cancellationToken).Returns(false);
        tagFailure.CategoryRepository.GetByIdAsync(categoryId, cancellationToken).Returns((Category?)Category(categoryId, tenantId));
        tagFailure.TagRepository.GetByIdAsync(tagId, cancellationToken).Returns((Tag?)Tag(tagId, otherTenantId));

        await Assert.ThrowsAsync<NotFoundException>(async () => await categoryFailure.CreateHandler.Handle(
            new CreateBlogCommand(tenantId, CreateDto(categoryIds: [categoryId], tagIds: [tagId])),
            cancellationToken));
        await Assert.ThrowsAsync<NotFoundException>(async () => await tagFailure.CreateHandler.Handle(
            new CreateBlogCommand(tenantId, CreateDto(categoryIds: [categoryId], tagIds: [tagId])),
            cancellationToken));

        await categoryFailure.BlogRepository.DidNotReceive().CreateAsync(Arg.Any<Blog>(), Arg.Any<CancellationToken>());
        await tagFailure.BlogRepository.DidNotReceive().CreateAsync(Arg.Any<Blog>(), Arg.Any<CancellationToken>());
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Create_WithCrossTenantOrWrongPurposeThumbnail_Fails(bool crossTenant)
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var context = TestContext(tenantId, cancellationToken);
        context.BlogRepository.SlugExistsAsync(tenantId, "community-update", null, cancellationToken).Returns(false);
        context.CategoryRepository.GetByIdAsync(categoryId, cancellationToken).Returns((Category?)Category(categoryId, tenantId));
        context.TagRepository.GetByIdAsync(tagId, cancellationToken).Returns((Tag?)Tag(tagId, tenantId));
        context.StorageRepository.GetByIdAsync(thumbnailId, cancellationToken).Returns(StorageObject(
            thumbnailId,
            crossTenant ? Guid.NewGuid() : tenantId,
            crossTenant ? StorageObjectPurposes.BlogImage : StorageObjectPurposes.ProfileImage));

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await context.CreateHandler.Handle(
            new CreateBlogCommand(tenantId, CreateDto(thumbnailStorageObjectId: thumbnailId, categoryIds: [categoryId], tagIds: [tagId])),
            cancellationToken));

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Errors.Any(error => error.PropertyName == "Blog.ThumbnailStorageObjectId")).IsTrue();
        await context.BlogRepository.DidNotReceive().CreateAsync(Arg.Any<Blog>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_ChecksSlugExcludingCurrentIdAndReplacesJoins()
    {
        var tenantId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var oldTagId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var newTagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var blog = Blog.Create(tenantId, "Old Title", "old-title", "old body");
        blog.ConcurrencyStamp = Guid.NewGuid();
        blog.ReplaceCategories([oldCategoryId]);
        blog.ReplaceTags([oldTagId]);
        var context = TestContext(tenantId, cancellationToken);
        Blog? updated = null;
        context.BlogRepository.GetByTenantAndIdAsync(tenantId, blog.Id, cancellationToken).Returns(blog);
        context.BlogRepository.SlugExistsAsync(tenantId, "updated-title", blog.Id, cancellationToken).Returns(false);
        context.BlogRepository.UpdateAsync(Arg.Do<Blog>(candidate => updated = candidate), cancellationToken)
            .Returns(call => Task.FromResult<Blog>(call.Arg<Blog>()!));
        context.CategoryRepository.GetByIdAsync(newCategoryId, cancellationToken).Returns((Category?)Category(newCategoryId, tenantId));
        context.TagRepository.GetByIdAsync(newTagId, cancellationToken).Returns((Tag?)Tag(newTagId, tenantId));
        context.StorageRepository.GetByIdAsync(thumbnailId, cancellationToken).Returns(StorageObject(thumbnailId, tenantId, StorageObjectPurposes.BlogImage));

        var result = await context.UpdateHandler.Handle(
            new UpdateBlogCommand(
                tenantId,
                blog.Id,
                UpdateDto(
                    title: "Updated Title",
                    slug: null,
                    expectedConcurrencyStamp: blog.ConcurrencyStamp,
                    thumbnailStorageObjectId: thumbnailId,
                    categoryIds: [newCategoryId, newCategoryId],
                    tagIds: [newTagId, newTagId])),
            cancellationToken);

        await Assert.That(result.Id).IsEqualTo(blog.Id);
        await Assert.That(updated).IsSameReferenceAs(blog);
        await Assert.That(updated!.Title).IsEqualTo("Updated Title");
        await Assert.That(updated.Slug).IsEqualTo("updated-title");
        await Assert.That(updated.CategoryLinks.Select(link => link.CategoryId).ToList()).IsEquivalentTo([newCategoryId]);
        await Assert.That(updated.TagLinks.Select(link => link.TagId).ToList()).IsEquivalentTo([newTagId]);
        await context.BlogRepository.Received(1).GetByTenantAndIdAsync(tenantId, blog.Id, cancellationToken);
        await context.BlogRepository.Received(1).SlugExistsAsync(tenantId, "updated-title", blog.Id, cancellationToken);
        await context.BlogRepository.Received(1).UpdateAsync(blog, cancellationToken);
        await context.CategoryRepository.Received(1).GetByIdAsync(newCategoryId, cancellationToken);
        await context.TagRepository.Received(1).GetByIdAsync(newTagId, cancellationToken);
        await context.StorageRepository.Received(1).GetByIdAsync(thumbnailId, cancellationToken);
    }

    [Test]
    public async Task Update_WithStaleConcurrencyStamp_FailsBeforeSlugRelationsAndMutation()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var blog = Blog.Create(tenantId, "Old Title", "old-title", "old body");
        blog.ConcurrencyStamp = Guid.NewGuid();
        var context = TestContext(tenantId, cancellationToken);
        context.BlogRepository.GetByTenantAndIdAsync(tenantId, blog.Id, cancellationToken).Returns(blog);

        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(async () => await context.UpdateHandler.Handle(
            new UpdateBlogCommand(
                tenantId,
                blog.Id,
                UpdateDto(
                    expectedConcurrencyStamp: Guid.NewGuid(),
                    thumbnailStorageObjectId: thumbnailId,
                    categoryIds: [categoryId],
                    tagIds: [tagId])),
            cancellationToken));

        await Assert.That(exception).IsNotNull();
        await Assert.That(blog.Title).IsEqualTo("Old Title");
        await Assert.That(blog.Slug).IsEqualTo("old-title");
        await context.BlogRepository.Received(1).GetByTenantAndIdAsync(tenantId, blog.Id, cancellationToken);
        await context.BlogRepository.DidNotReceive().SlugExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await context.BlogRepository.DidNotReceive().UpdateAsync(Arg.Any<Blog>(), Arg.Any<CancellationToken>());
        await context.CategoryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await context.TagRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await context.StorageRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PublishUnpublishAndDelete_PassObservableLifecycleStateToUpdateAsync()
    {
        var tenantId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var publishBlog = Blog.Create(tenantId, "Publish Me", "publish-me", "body");
        var unpublishBlog = Blog.Create(tenantId, "Unpublish Me", "unpublish-me", "body");
        unpublishBlog.Publish(DateTime.UtcNow.AddDays(-1));
        var deleteBlog = Blog.Create(tenantId, "Delete Me", "delete-me", "body");
        Blog? published = null;
        Blog? unpublished = null;
        Blog? deleted = null;
        var publishRepository = BlogRepositoryReturning(publishBlog, tenantId, cancellationToken, blog => published = blog);
        var unpublishRepository = BlogRepositoryReturning(unpublishBlog, tenantId, cancellationToken, blog => unpublished = blog);
        var deleteRepository = BlogRepositoryReturning(deleteBlog, tenantId, cancellationToken, blog => deleted = blog);

        await new PublishBlogCommandHandler(publishRepository).Handle(new PublishBlogCommand(tenantId, publishBlog.Id), cancellationToken);
        await new UnpublishBlogCommandHandler(unpublishRepository).Handle(new UnpublishBlogCommand(tenantId, unpublishBlog.Id), cancellationToken);
        await new DeleteBlogCommandHandler(deleteRepository).Handle(new DeleteBlogCommand(tenantId, deleteBlog.Id), cancellationToken);

        await Assert.That(published).IsSameReferenceAs(publishBlog);
        await Assert.That(published!.PublishedAtUtc.HasValue).IsTrue();
        await Assert.That(published.IsDeleted).IsFalse();
        await Assert.That(unpublished).IsSameReferenceAs(unpublishBlog);
        await Assert.That(unpublished!.PublishedAtUtc.HasValue).IsFalse();
        await Assert.That(unpublished.IsDeleted).IsFalse();
        await Assert.That(deleted).IsSameReferenceAs(deleteBlog);
        await Assert.That(deleted!.PublishedAtUtc.HasValue).IsFalse();
        await Assert.That(deleted.IsDeleted).IsTrue();
        await publishRepository.Received(1).UpdateAsync(publishBlog, cancellationToken);
        await unpublishRepository.Received(1).UpdateAsync(unpublishBlog, cancellationToken);
        await deleteRepository.Received(1).UpdateAsync(deleteBlog, cancellationToken);
    }

    [Test]
    public async Task Create_WhenCategoryLookupCancels_PropagatesTokenAndStops()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var context = TestContext(tenantId, cancellationTokenSource.Token);
        context.BlogRepository.SlugExistsAsync(tenantId, "community-update", null, cancellationTokenSource.Token).Returns(false);
        context.CategoryRepository.GetByIdAsync(categoryId, cancellationTokenSource.Token)
            .Returns(_ =>
            {
                cancellationTokenSource.Cancel();
                return Task.FromCanceled<Category?>(cancellationTokenSource.Token);
            });

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await context.CreateHandler.Handle(
            new CreateBlogCommand(
                tenantId,
                CreateDto(thumbnailStorageObjectId: thumbnailId, categoryIds: [categoryId], tagIds: [tagId])),
            cancellationTokenSource.Token));

        await context.CategoryRepository.Received(1).GetByIdAsync(categoryId, cancellationTokenSource.Token);
        await context.TagRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await context.StorageRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static BlogRepositorySet TestContext(Guid tenantId, CancellationToken cancellationToken)
    {
        var blogRepository = Substitute.For<IBlogRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var tagRepository = Substitute.For<ITagRepository>();
        var storageRepository = Substitute.For<IStorageObjectRepository>();
        return new BlogRepositorySet(
            blogRepository,
            categoryRepository,
            tagRepository,
            storageRepository,
            new CreateBlogCommandHandler(blogRepository, categoryRepository, tagRepository, storageRepository),
            new UpdateBlogCommandHandler(blogRepository, categoryRepository, tagRepository, storageRepository));
    }

    private static IBlogRepository BlogRepositoryReturning(
        Blog blog,
        Guid tenantId,
        CancellationToken cancellationToken,
        Action<Blog> captureUpdate)
    {
        var repository = Substitute.For<IBlogRepository>();
        repository.GetByTenantAndIdAsync(tenantId, blog.Id, cancellationToken).Returns(blog);
        repository.UpdateAsync(Arg.Do<Blog>(captureUpdate), cancellationToken).Returns(blog);
        return repository;
    }

    private static CreateBlogDto CreateDto(
        string title = "Community Update",
        string? slug = "community-update",
        Guid? thumbnailStorageObjectId = null,
        IReadOnlyList<Guid>? categoryIds = null,
        IReadOnlyList<Guid>? tagIds = null) =>
        new()
        {
            Title = title,
            Slug = slug,
            ContentMarkdown = "## Body",
            ThumbnailStorageObjectId = thumbnailStorageObjectId,
            CategoryIds = categoryIds ?? [Guid.NewGuid()],
            TagIds = tagIds ?? [Guid.NewGuid()]
        };

    private static UpdateBlogDto UpdateDto(
        string title = "Community Update",
        string? slug = "community-update",
        Guid? expectedConcurrencyStamp = null,
        Guid? thumbnailStorageObjectId = null,
        IReadOnlyList<Guid>? categoryIds = null,
        IReadOnlyList<Guid>? tagIds = null) =>
        new()
        {
            Title = title,
            Slug = slug,
            ContentMarkdown = "## Updated Body",
            ExpectedConcurrencyStamp = expectedConcurrencyStamp ?? Guid.NewGuid(),
            ThumbnailStorageObjectId = thumbnailStorageObjectId,
            CategoryIds = categoryIds ?? [Guid.NewGuid()],
            TagIds = tagIds ?? [Guid.NewGuid()]
        };

    private static Category Category(Guid id, Guid tenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        Tenant = null!,
        MasterCode = "category",
        FullName = "Category"
    };

    private static Tag Tag(Guid id, Guid tenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        Tenant = null!,
        MasterCode = "tag",
        FullName = "Tag"
    };

    private static StorageObject StorageObject(Guid id, Guid tenantId, string purpose) => new()
    {
        Id = id,
        TenantId = tenantId,
        Tenant = null!,
        FileTypeId = 1,
        FileType = null!,
        Uri = $"local://{id}",
        Provider = StorageProviders.Local,
        FullName = "thumbnail.png",
        SafeDisplayName = "thumbnail.png",
        Extension = ".png",
        Visibility = StorageObjectVisibilities.PublicImage,
        Purpose = purpose,
        LifecycleState = StorageObjectLifecycleStates.Active
    };

    private sealed record BlogRepositorySet(
        IBlogRepository BlogRepository,
        ICategoryRepository CategoryRepository,
        ITagRepository TagRepository,
        IStorageObjectRepository StorageRepository,
        CreateBlogCommandHandler CreateHandler,
        UpdateBlogCommandHandler UpdateHandler);
}
