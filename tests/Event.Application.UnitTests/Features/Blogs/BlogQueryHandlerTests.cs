// ABOUTME: Unit tests for public Blog query handlers using repository substitutes and real specifications.
// ABOUTME: Covers published list paging, DTO mapping, detail visibility, slug normalization, and cancellation flow.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Features.Blogs.Handlers.Queries;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Specifications;
using Explore.Domain;
using NSubstitute;

namespace Event.Application.UnitTests.Features.Blogs;

public sealed class BlogQueryHandlerTests
{
    [Test]
    public async Task GetPublishedBlogs_UsesRealCountPagingMappingAndSpecification()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var visibleOlder = PublishedBlog(tenantId, "Community Search Older", "older", now.AddDays(-3), [categoryId], [tagId]);
        var visibleMiddle = PublishedBlog(tenantId, "Community Search Middle", "middle", now.AddDays(-2), [categoryId], [tagId]);
        var visibleNewest = PublishedBlog(tenantId, "Community Search Newest", "newest", now.AddDays(-1), [categoryId], [tagId]);
        var draft = Blog.Create(tenantId, "Community Search Draft", "draft", "draft body");
        draft.ReplaceCategories([categoryId]);
        draft.ReplaceTags([tagId]);
        var otherTenant = PublishedBlog(otherTenantId, "Community Search Other", "other", now.AddDays(-1), [categoryId], [tagId]);
        var wrongCategory = PublishedBlog(tenantId, "Community Search Wrong Category", "wrong-category", now.AddDays(-1), [Guid.NewGuid()], [tagId]);
        var wrongTag = PublishedBlog(tenantId, "Community Search Wrong Tag", "wrong-tag", now.AddDays(-1), [categoryId], [Guid.NewGuid()]);
        var wrongSearch = PublishedBlog(tenantId, "Friday Notes", "friday", now.AddDays(-1), [categoryId], [tagId]);
        Blog[] blogs = [visibleOlder, visibleMiddle, visibleNewest, draft, otherTenant, wrongCategory, wrongTag, wrongSearch];
        var repository = Substitute.For<IBlogRepository>();
        var cancellationToken = new CancellationTokenSource().Token;
        BlogQuerySpecification? countSpecification = null;
        BlogQuerySpecification? listSpecification = null;
        int capturedSkip = -1;
        int capturedTake = -1;

        repository.CountAsync(Arg.Any<BlogQuerySpecification>(), cancellationToken)
            .Returns(call =>
            {
                var specification = call.Arg<BlogQuerySpecification>();
                ArgumentNullException.ThrowIfNull(specification);
                countSpecification = specification;
                return Task.FromResult(specification.Apply(blogs.AsQueryable()).Count());
            });
        repository.ListAsync(Arg.Any<BlogQuerySpecification>(), Arg.Any<int>(), Arg.Any<int>(), cancellationToken)
            .Returns(call =>
            {
                var specification = call.Arg<BlogQuerySpecification>();
                ArgumentNullException.ThrowIfNull(specification);
                listSpecification = specification;
                capturedSkip = call.ArgAt<int>(1);
                capturedTake = call.ArgAt<int>(2);
                IReadOnlyList<Blog> page = specification.Apply(blogs.AsQueryable())
                    .Skip(capturedSkip)
                    .Take(capturedTake)
                    .ToList();
                return Task.FromResult(page);
            });
        var handler = new GetPublishedBlogsQueryHandler(repository);

        var result = await handler.Handle(
            new GetPublishedBlogsQuery(tenantId, Page: 2, PageSize: 2, categoryId, tagId, "Search"),
            cancellationToken);

        await Assert.That(result.TotalCount).IsEqualTo(3);
        await Assert.That(result.PageNumber).IsEqualTo(2);
        await Assert.That(result.PageSize).IsEqualTo(2);
        await Assert.That(result.Items).Count().IsEqualTo(1);
        await Assert.That(result.Items[0].Id).IsEqualTo(visibleOlder.Id);
        await Assert.That(result.Items[0].TenantId).IsEqualTo(tenantId);
        await Assert.That(result.Items[0].Title).IsEqualTo(visibleOlder.Title);
        await Assert.That(result.Items[0].Slug).IsEqualTo(visibleOlder.Slug);
        await Assert.That(result.Items[0].CategoryIds).IsEquivalentTo([categoryId]);
        await Assert.That(result.Items[0].TagIds).IsEquivalentTo([tagId]);
        await Assert.That(countSpecification).IsNotNull();
        await Assert.That(listSpecification).IsNotNull();
        await Assert.That(countSpecification!.Apply(blogs.AsQueryable()).Select(blog => blog.Id).ToList())
            .IsEquivalentTo([visibleNewest.Id, visibleMiddle.Id, visibleOlder.Id]);
        await Assert.That(capturedSkip).IsEqualTo(2);
        await Assert.That(capturedTake).IsEqualTo(2);
        await repository.Received(1).CountAsync(Arg.Any<BlogQuerySpecification>(), cancellationToken);
        await repository.Received(1).ListAsync(Arg.Any<BlogQuerySpecification>(), 2, 2, cancellationToken);
    }

    [Test]
    public async Task GetBlogsForManagement_UsesTenantSearchPagingMappingAndIncludesDrafts()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var older = PublishedBlog(tenantId, "Management Search Older", "older", now.AddDays(-3), [], []);
        var newest = PublishedBlog(tenantId, "Management Search Newest", "newest", now.AddDays(-1), [], []);
        var draft = Blog.Create(tenantId, "Management Search Draft", "draft", "draft body");
        var otherTenant = PublishedBlog(otherTenantId, "Management Search Other", "other", now.AddDays(-1), [], []);
        var wrongSearch = PublishedBlog(tenantId, "Friday Notes", "friday", now.AddDays(-1), [], []);
        Blog[] blogs = [older, newest, draft, otherTenant, wrongSearch];
        var repository = Substitute.For<IBlogRepository>();
        var cancellationToken = new CancellationTokenSource().Token;
        BlogQuerySpecification? countSpecification = null;
        BlogQuerySpecification? listSpecification = null;
        int capturedSkip = -1;
        int capturedTake = -1;

        repository.CountAsync(Arg.Any<BlogQuerySpecification>(), cancellationToken)
            .Returns(call =>
            {
                var specification = call.Arg<BlogQuerySpecification>();
                ArgumentNullException.ThrowIfNull(specification);
                countSpecification = specification;
                return Task.FromResult(specification.Apply(blogs.AsQueryable()).Count());
            });
        repository.ListAsync(Arg.Any<BlogQuerySpecification>(), Arg.Any<int>(), Arg.Any<int>(), cancellationToken)
            .Returns(call =>
            {
                var specification = call.Arg<BlogQuerySpecification>();
                ArgumentNullException.ThrowIfNull(specification);
                listSpecification = specification;
                capturedSkip = call.ArgAt<int>(1);
                capturedTake = call.ArgAt<int>(2);
                IReadOnlyList<Blog> page = specification.Apply(blogs.AsQueryable())
                    .Skip(capturedSkip)
                    .Take(capturedTake)
                    .ToList();
                return Task.FromResult(page);
            });
        var handler = new GetBlogsForManagementQueryHandler(repository);

        var result = await handler.Handle(new GetBlogsForManagementQuery(tenantId, Page: 2, PageSize: 2, "Search"), cancellationToken);

        await Assert.That(result.TotalCount).IsEqualTo(3);
        await Assert.That(result.PageNumber).IsEqualTo(2);
        await Assert.That(result.PageSize).IsEqualTo(2);
        await Assert.That(result.Items).Count().IsEqualTo(1);
        await Assert.That(result.Items[0].Id).IsEqualTo(draft.Id);
        await Assert.That(result.Items[0].TenantId).IsEqualTo(tenantId);
        await Assert.That(result.Items[0].Title).IsEqualTo(draft.Title);
        await Assert.That(result.Items[0].Slug).IsEqualTo(draft.Slug);
        await Assert.That(result.Items[0].PublishedAtUtc).IsNull();
        await Assert.That(countSpecification).IsNotNull();
        await Assert.That(listSpecification).IsNotNull();
        await Assert.That(countSpecification!.Apply(blogs.AsQueryable()).Select(blog => blog.Id).ToList())
            .IsEquivalentTo([newest.Id, older.Id, draft.Id]);
        await Assert.That(capturedSkip).IsEqualTo(2);
        await Assert.That(capturedTake).IsEqualTo(2);
        await repository.Received(1).CountAsync(Arg.Any<BlogQuerySpecification>(), cancellationToken);
        await repository.Received(1).ListAsync(Arg.Any<BlogQuerySpecification>(), 2, 2, cancellationToken);
    }

    [Test]
    public async Task GetPublishedBlogBySlug_MapsPublishedPostAndHidesNonPublicPosts()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var repository = Substitute.For<IBlogRepository>();
        var published = PublishedBlog(tenantId, "Published Post", "published-post", DateTime.UtcNow.AddDays(-1), [categoryId], [tagId], thumbnailId);
        published.ConcurrencyStamp = Guid.NewGuid();
        var draft = Blog.Create(tenantId, "Draft Post", "draft-post", "draft body");
        var future = PublishedBlog(tenantId, "Future Post", "future-post", DateTime.UtcNow.AddDays(1), [], []);
        var deleted = PublishedBlog(tenantId, "Deleted Post", "deleted-post", DateTime.UtcNow.AddDays(-1), [], []);
        deleted.MarkDeleted(null, DateTime.UtcNow);
        repository.GetByTenantAndSlugAsync(tenantId, "published-post", cancellationToken).Returns(published);
        repository.GetByTenantAndSlugAsync(tenantId, "draft-post", cancellationToken).Returns(draft);
        repository.GetByTenantAndSlugAsync(tenantId, "future-post", cancellationToken).Returns(future);
        repository.GetByTenantAndSlugAsync(tenantId, "deleted-post", cancellationToken).Returns(deleted);
        var handler = new GetPublishedBlogBySlugQueryHandler(repository);

        var mapped = await handler.Handle(new GetPublishedBlogBySlugQuery(tenantId, "Published Post"), cancellationToken);
        var draftResult = await handler.Handle(new GetPublishedBlogBySlugQuery(tenantId, "Draft Post"), cancellationToken);
        var futureResult = await handler.Handle(new GetPublishedBlogBySlugQuery(tenantId, "Future Post"), cancellationToken);
        var deletedResult = await handler.Handle(new GetPublishedBlogBySlugQuery(tenantId, "Deleted Post"), cancellationToken);
        var punctuationResult = await handler.Handle(new GetPublishedBlogBySlugQuery(tenantId, "!!!"), cancellationToken);

        await Assert.That(mapped).IsNotNull();
        await Assert.That(mapped!.Id).IsEqualTo(published.Id);
        await Assert.That(mapped.TenantId).IsEqualTo(tenantId);
        await Assert.That(mapped.Title).IsEqualTo(published.Title);
        await Assert.That(mapped.Slug).IsEqualTo("published-post");
        await Assert.That(mapped.ContentMarkdown).IsEqualTo(published.ContentMarkdown);
        await Assert.That(mapped.ConcurrencyStamp).IsEqualTo(published.ConcurrencyStamp);
        await Assert.That(mapped.ThumbnailStorageObjectId).IsEqualTo(thumbnailId);
        await Assert.That(mapped.CategoryIds).IsEquivalentTo([categoryId]);
        await Assert.That(mapped.TagIds).IsEquivalentTo([tagId]);
        await Assert.That(draftResult).IsNull();
        await Assert.That(futureResult).IsNull();
        await Assert.That(deletedResult).IsNull();
        await Assert.That(punctuationResult).IsNull();
        await repository.Received(1).GetByTenantAndSlugAsync(tenantId, "published-post", cancellationToken);
        await repository.Received(1).GetByTenantAndSlugAsync(tenantId, "draft-post", cancellationToken);
        await repository.Received(1).GetByTenantAndSlugAsync(tenantId, "future-post", cancellationToken);
        await repository.Received(1).GetByTenantAndSlugAsync(tenantId, "deleted-post", cancellationToken);
        await repository.DidNotReceive().GetByTenantAndSlugAsync(tenantId, string.Empty, cancellationToken);
    }

    [Test]
    public async Task GetBlogByIdForManagement_MapsDraftAndReturnsNullWhenMissing()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var thumbnailId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var repository = Substitute.For<IBlogRepository>();
        var draft = Blog.Create(tenantId, "Draft Post", "draft-post", "draft body", thumbnailId);
        draft.ConcurrencyStamp = Guid.NewGuid();
        draft.ReplaceCategories([categoryId]);
        draft.ReplaceTags([tagId]);
        var missingBlogId = Guid.NewGuid();
        repository.GetByTenantAndIdAsync(tenantId, draft.Id, cancellationToken).Returns(draft);
        repository.GetByTenantAndIdAsync(tenantId, missingBlogId, cancellationToken).Returns((Blog?)null);
        var handler = new GetBlogByIdForManagementQueryHandler(repository);

        var mapped = await handler.Handle(new GetBlogByIdForManagementQuery(tenantId, draft.Id), cancellationToken);
        var missing = await handler.Handle(new GetBlogByIdForManagementQuery(tenantId, missingBlogId), cancellationToken);

        await Assert.That(mapped).IsNotNull();
        await Assert.That(mapped!.Id).IsEqualTo(draft.Id);
        await Assert.That(mapped.TenantId).IsEqualTo(tenantId);
        await Assert.That(mapped.Title).IsEqualTo(draft.Title);
        await Assert.That(mapped.Slug).IsEqualTo(draft.Slug);
        await Assert.That(mapped.ContentMarkdown).IsEqualTo(draft.ContentMarkdown);
        await Assert.That(mapped.ConcurrencyStamp).IsEqualTo(draft.ConcurrencyStamp);
        await Assert.That(mapped.ThumbnailStorageObjectId).IsEqualTo(thumbnailId);
        await Assert.That(mapped.PublishedAtUtc).IsNull();
        await Assert.That(mapped.CategoryIds).IsEquivalentTo([categoryId]);
        await Assert.That(mapped.TagIds).IsEquivalentTo([tagId]);
        await Assert.That(missing).IsNull();
        await repository.Received(1).GetByTenantAndIdAsync(tenantId, draft.Id, cancellationToken);
        await repository.Received(1).GetByTenantAndIdAsync(tenantId, missingBlogId, cancellationToken);
    }

    private static Blog PublishedBlog(
        Guid tenantId,
        string title,
        string slug,
        DateTime publishedAtUtc,
        IReadOnlyCollection<Guid> categoryIds,
        IReadOnlyCollection<Guid> tagIds,
        Guid? thumbnailStorageObjectId = null)
    {
        var blog = Blog.Create(tenantId, title, slug, $"## {title}", thumbnailStorageObjectId);
        blog.Publish(DateTime.SpecifyKind(publishedAtUtc, DateTimeKind.Utc));
        blog.ReplaceCategories(categoryIds);
        blog.ReplaceTags(tagIds);
        return blog;
    }
}
