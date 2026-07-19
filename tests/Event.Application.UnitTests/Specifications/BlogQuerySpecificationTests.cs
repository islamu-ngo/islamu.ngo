// ABOUTME: Unit tests for BlogQuerySpecification immutable filtering and sorting behavior.
// ABOUTME: Exercises tenant, publication, category/tag, and title filters without EF dependencies.

using Explore.Application.Specifications;
using Explore.Domain;

namespace Event.Application.UnitTests.Specifications;

public sealed class BlogQuerySpecificationTests
{
    [Test]
    public async Task ForTenantAndPublishedAt_FiltersToPublishedTenantBlogsOnly()
    {
        Guid tenantId = Guid.NewGuid();
        Blog visible = Blog.Create(tenantId, "Visible", "visible", "body");
        Blog draft = Blog.Create(tenantId, "Draft", "draft", "body");
        Blog otherTenant = Blog.Create(Guid.NewGuid(), "Other", "other", "body");
        visible.Publish(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
        otherTenant.Publish(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));

        IQueryable<Blog> query = BlogQuerySpecification.Empty
            .ForTenant(tenantId)
            .PublishedAt(new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc))
            .Apply(new[] { visible, draft, otherTenant }.AsQueryable());

        await Assert.That(query.ToList()).IsEquivalentTo([visible]);
    }

    [Test]
    public async Task WithCategoryAndTag_FiltersJoinEntities()
    {
        Blog matching = Blog.Create(Guid.NewGuid(), "Match", "match", "body");
        Blog other = Blog.Create(matching.TenantId, "Other", "other", "body");
        Guid categoryId = Guid.NewGuid();
        Guid tagId = Guid.NewGuid();
        matching.ReplaceCategories([categoryId]);
        matching.ReplaceTags([tagId]);
        other.ReplaceCategories([Guid.NewGuid()]);
        other.ReplaceTags([tagId]);

        IQueryable<Blog> query = BlogQuerySpecification.Empty
            .WithCategory(categoryId)
            .WithTag(tagId)
            .Apply(new[] { matching, other }.AsQueryable());

        await Assert.That(query.ToList()).IsEquivalentTo([matching]);
    }

    [Test]
    public async Task SortByPublishedAtDescending_ReturnsNewSpecificationAndOrdersResults()
    {
        Blog older = Blog.Create(Guid.NewGuid(), "Older", "older", "body");
        Blog newer = Blog.Create(older.TenantId, "Newer", "newer", "body");
        older.Publish(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
        newer.Publish(new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc));

        BlogQuerySpecification specification = BlogQuerySpecification.Empty.SortByPublishedAtDescending();

        await Assert.That(BlogQuerySpecification.Empty.HasSort).IsFalse();
        await Assert.That(specification.HasSort).IsTrue();

        List<Blog> results = specification.Apply(new[] { older, newer }.AsQueryable()).ToList();

        await Assert.That(results[0]).IsEqualTo(newer);
        await Assert.That(results[1]).IsEqualTo(older);
    }
}
