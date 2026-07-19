// ABOUTME: Unit tests for the Blog aggregate lifecycle and category/tag replacement behavior.
// ABOUTME: Covers tenant validation, UTC publication rules, raw Markdown storage, and soft-delete invariants.

namespace Event.Domain.UnitTests.Entities;

public sealed class BlogTests
{
    [Test]
    public async Task Create_WithValidValues_StoresRawMarkdownAndStartsUnpublished()
    {
        Guid tenantId = Guid.NewGuid();
        const string markdown = "# Title\n\n<script>alert('raw')</script>";

        Blog blog = Blog.Create(tenantId, "  My Post  ", "my-post", markdown);

        await Assert.That(blog.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(blog.TenantId).IsEqualTo(tenantId);
        await Assert.That(blog.Title).IsEqualTo("My Post");
        await Assert.That(blog.Slug).IsEqualTo("my-post");
        await Assert.That(blog.ContentMarkdown).IsEqualTo(markdown);
        await Assert.That(blog.PublishedAtUtc).IsNull();
    }

    [Test]
    public async Task Create_WithEmptyTenant_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            _ = Blog.Create(Guid.Empty, "Title", "title", "body");
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task CreateAndUpdate_WithCanonicalLatinAndArabicSlugs_Accepts()
    {
        var tenantId = Guid.NewGuid();
        var blog = Blog.Create(tenantId, "Title", "community-update-2026", "body");

        blog.Update("Arabic", "السلام-عليكم-2026", "body");

        await Assert.That(blog.Slug).IsEqualTo("السلام-عليكم-2026");
    }

    [Test]
    [Arguments("Community-Update")]
    [Arguments("community update")]
    [Arguments("community_update")]
    [Arguments("-community")]
    [Arguments("community-")]
    [Arguments("community--update")]
    public async Task Create_WithNonCanonicalSlug_Throws(string slug)
    {
        await AssertInvalidSlug(slug);
    }

    [Test]
    public async Task Create_WithOverlengthSlug_Throws()
    {
        await AssertInvalidSlug(new string('a', 201));
    }

    [Test]
    public async Task Publish_WithLocalTimestamp_Throws()
    {
        Blog blog = Blog.Create(Guid.NewGuid(), "Title", "title", "body");

        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            blog.Publish(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local));
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Publish_WhenAlreadyPublished_Throws()
    {
        Blog blog = Blog.Create(Guid.NewGuid(), "Title", "title", "body");
        blog.Publish(DateTime.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            blog.Publish(DateTime.UtcNow.AddMinutes(1));
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Unpublish_WhenPublished_ClearsPublishedAt()
    {
        Blog blog = Blog.Create(Guid.NewGuid(), "Title", "title", "body");
        blog.Publish(DateTime.UtcNow);

        blog.Unpublish();

        await Assert.That(blog.PublishedAtUtc).IsNull();
    }

    [Test]
    public async Task ReplaceCategoriesAndTags_DeduplicatesIds()
    {
        Blog blog = Blog.Create(Guid.NewGuid(), "Title", "title", "body");
        Guid categoryId = Guid.NewGuid();
        Guid tagId = Guid.NewGuid();

        blog.ReplaceCategories([categoryId, categoryId]);
        blog.ReplaceTags([tagId, tagId]);

        await Assert.That(blog.CategoryLinks.Count).IsEqualTo(1);
        await Assert.That(blog.CategoryLinks.Single().CategoryId).IsEqualTo(categoryId);
        await Assert.That(blog.TagLinks.Count).IsEqualTo(1);
        await Assert.That(blog.TagLinks.Single().TagId).IsEqualTo(tagId);
    }

    [Test]
    public async Task MarkDeleted_UnpublishesAndBlocksFurtherChanges()
    {
        Blog blog = Blog.Create(Guid.NewGuid(), "Title", "title", "body");
        blog.Publish(DateTime.UtcNow);

        blog.MarkDeleted(Guid.NewGuid(), DateTime.UtcNow);

        await Assert.That(blog.IsDeleted).IsTrue();
        await Assert.That(blog.PublishedAtUtc).IsNull();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            blog.Update("Other", "other", "body");
            return Task.CompletedTask;
        });
    }

    private static async Task AssertInvalidSlug(string slug)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            _ = Blog.Create(Guid.NewGuid(), "Title", slug, "body");
            return Task.CompletedTask;
        });
    }
}
