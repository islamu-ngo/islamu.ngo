// ABOUTME: Integration test source for tenant-scoped Blog repository persistence behavior.
// ABOUTME: Covers tenant isolation, specifications, soft-deleted slug checks, and composite relationship constraints.

using Event.Persistence.IntegrationTests.Fixtures;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.Specifications;
using Explore.Domain;
using Explore.Domain.Enums;
using Explore.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Event.Persistence.IntegrationTests.Repositories;

[ClassDataSource<PostgreSqlContainerFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("PersistenceDb")]
public sealed class BlogRepositoryTests(PostgreSqlContainerFixture fixture)
{
    [Test]
    public async Task Reads_WithTenantFilter_ReturnOnlyAmbientTenantBlogs()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenantA = CreateTenant("blog-tenant-a");
        var tenantB = CreateTenant("blog-tenant-b");
        seedContext.Tenants.AddRange(tenantA, tenantB);
        await seedContext.SaveChangesAsync();

        var visible = Blog.Create(tenantA.Id, "Visible", "visible", "body");
        var hidden = Blog.Create(tenantB.Id, "Hidden", "hidden", "body");
        seedContext.Blogs.AddRange(visible, hidden);
        await seedContext.SaveChangesAsync();

        await using var tenantAContext = fixture.CreateTenantFilteredDbContext(new TestTenantContext(tenantA.Id));
        var repository = new BlogRepository(tenantAContext);

        var results = await repository.ListAsync(
            BlogQuerySpecification.Empty.ForTenant(tenantA.Id),
            skip: 0,
            take: 20,
            CancellationToken.None);

        await Assert.That(results.Select(blog => blog.Id)).IsEquivalentTo([visible.Id]);
    }

    [Test]
    public async Task ListAsync_AppliesPublishedAndJoinSpecifications()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenant = CreateTenant("blog-spec");
        seedContext.Tenants.Add(tenant);
        await seedContext.SaveChangesAsync();

        var category = CreateCategory(tenant.Id, "blog-category");
        var tag = CreateTag(tenant.Id, "blog-tag");
        seedContext.Categories.Add(category);
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var matching = Blog.Create(tenant.Id, "Published Match", "published-match", "body");
        matching.Publish(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
        matching.ReplaceCategories([category.Id]);
        matching.ReplaceTags([tag.Id]);
        var draft = Blog.Create(tenant.Id, "Draft", "draft", "body");
        draft.ReplaceCategories([category.Id]);
        draft.ReplaceTags([tag.Id]);
        seedContext.Blogs.AddRange(matching, draft);
        await seedContext.SaveChangesAsync();

        var repository = new BlogRepository(seedContext);
        var results = await repository.ListAsync(
            BlogQuerySpecification.Empty
                .ForTenant(tenant.Id)
                .PublishedAt(new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc))
                .WithCategory(category.Id)
                .WithTag(tag.Id),
            skip: 0,
            take: 20,
            CancellationToken.None);

        await Assert.That(results.Select(blog => blog.Id)).IsEquivalentTo([matching.Id]);
        await Assert.That(results.Single().CategoryLinks.Select(link => link.CategoryId)).IsEquivalentTo([category.Id]);
        await Assert.That(results.Single().TagLinks.Select(link => link.TagId)).IsEquivalentTo([tag.Id]);
    }

    [Test]
    public async Task CountAsync_AppliesPublishedCategoryTagFiltersWithoutPagination()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenantA = CreateTenant("blog-count-a");
        var tenantB = CreateTenant("blog-count-b");
        seedContext.Tenants.AddRange(tenantA, tenantB);
        await seedContext.SaveChangesAsync();

        var category = CreateCategory(tenantA.Id, "count-category");
        var tag = CreateTag(tenantA.Id, "count-tag");
        var otherTenantCategory = CreateCategory(tenantB.Id, "other-count-category");
        var otherTenantTag = CreateTag(tenantB.Id, "other-count-tag");
        seedContext.Categories.AddRange(category, otherTenantCategory);
        seedContext.Tags.AddRange(tag, otherTenantTag);
        await seedContext.SaveChangesAsync();

        var now = new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc);
        var matchingA = Blog.Create(tenantA.Id, "Published Match A", "published-match-a", "body");
        matchingA.Publish(now.AddDays(-2));
        matchingA.ReplaceCategories([category.Id]);
        matchingA.ReplaceTags([tag.Id]);
        var matchingB = Blog.Create(tenantA.Id, "Published Match B", "published-match-b", "body");
        matchingB.Publish(now.AddDays(-1));
        matchingB.ReplaceCategories([category.Id]);
        matchingB.ReplaceTags([tag.Id]);
        var draft = Blog.Create(tenantA.Id, "Draft Match", "draft-match", "body");
        draft.ReplaceCategories([category.Id]);
        draft.ReplaceTags([tag.Id]);
        var otherTenant = Blog.Create(tenantB.Id, "Other Tenant Match", "other-tenant-match", "body");
        otherTenant.Publish(now.AddDays(-1));
        otherTenant.ReplaceCategories([otherTenantCategory.Id]);
        otherTenant.ReplaceTags([otherTenantTag.Id]);
        seedContext.Blogs.AddRange(matchingA, matchingB, draft, otherTenant);
        await seedContext.SaveChangesAsync();

        await using var tenantAContext = fixture.CreateTenantFilteredDbContext(new TestTenantContext(tenantA.Id));
        var repository = new BlogRepository(tenantAContext);
        var specification = BlogQuerySpecification.Empty
            .ForTenant(tenantA.Id)
            .PublishedAt(now)
            .WithCategory(category.Id)
            .WithTag(tag.Id);

        var pagedResults = await repository.ListAsync(specification, skip: 0, take: 1, CancellationToken.None);
        var totalCount = await repository.CountAsync(specification, CancellationToken.None);

        await Assert.That(pagedResults).Count().IsEqualTo(1);
        await Assert.That(totalCount).IsEqualTo(2);
    }

    [Test]
    public async Task SlugExistsAsync_IncludesSoftDeletedRowsWithinTenantOnly()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenantA = CreateTenant("blog-slug-a");
        var tenantB = CreateTenant("blog-slug-b");
        seedContext.Tenants.AddRange(tenantA, tenantB);
        await seedContext.SaveChangesAsync();

        var deleted = Blog.Create(tenantA.Id, "Deleted", "same-slug", "body");
        deleted.MarkDeleted(null, new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc));
        var otherTenant = Blog.Create(tenantB.Id, "Other", "same-slug", "body");
        seedContext.Blogs.AddRange(deleted, otherTenant);
        await seedContext.SaveChangesAsync();

        await using var tenantAContext = fixture.CreateTenantFilteredDbContext(new TestTenantContext(tenantA.Id));
        var repository = new BlogRepository(tenantAContext);

        await Assert.That(await repository.SlugExistsAsync(tenantA.Id, "same-slug", null, CancellationToken.None)).IsTrue();
        await Assert.That(await repository.SlugExistsAsync(tenantA.Id, "same-slug", deleted.Id, CancellationToken.None)).IsFalse();
        await Assert.That(await repository.SlugExistsAsync(tenantA.Id, "other-slug", null, CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task UpdateAsync_WithDetachedAggregate_ReplacesJoinRowsWithoutDuplicates()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenant = CreateTenant("blog-update");
        seedContext.Tenants.Add(tenant);
        await seedContext.SaveChangesAsync();

        var staleCategory = CreateCategory(tenant.Id, "stale-category");
        var keptCategory = CreateCategory(tenant.Id, "kept-category");
        var addedCategory = CreateCategory(tenant.Id, "added-category");
        var staleTag = CreateTag(tenant.Id, "stale-tag");
        var keptTag = CreateTag(tenant.Id, "kept-tag");
        var addedTag = CreateTag(tenant.Id, "added-tag");
        seedContext.Categories.AddRange(staleCategory, keptCategory, addedCategory);
        seedContext.Tags.AddRange(staleTag, keptTag, addedTag);
        await seedContext.SaveChangesAsync();

        var blog = Blog.Create(tenant.Id, "Original", "original", "body");
        blog.ReplaceCategories([staleCategory.Id, keptCategory.Id]);
        blog.ReplaceTags([staleTag.Id, keptTag.Id]);
        seedContext.Blogs.Add(blog);
        await seedContext.SaveChangesAsync();
        seedContext.ChangeTracker.Clear();

        var repository = new BlogRepository(seedContext);
        var detached = await repository.GetByTenantAndIdAsync(tenant.Id, blog.Id, CancellationToken.None);

        detached!.Update("Updated", "updated", "updated body");
        detached.ReplaceCategories([keptCategory.Id, addedCategory.Id, keptCategory.Id]);
        detached.ReplaceTags([keptTag.Id, addedTag.Id, keptTag.Id]);

        await repository.UpdateAsync(detached, CancellationToken.None);
        seedContext.ChangeTracker.Clear();

        var categoryIds = await seedContext.BlogCategories
            .AsNoTracking()
            .Where(link => link.BlogId == blog.Id)
            .Select(link => link.CategoryId)
            .ToListAsync();
        var tagIds = await seedContext.BlogTags
            .AsNoTracking()
            .Where(link => link.BlogId == blog.Id)
            .Select(link => link.TagId)
            .ToListAsync();

        await Assert.That(categoryIds).IsEquivalentTo([keptCategory.Id, addedCategory.Id]);
        await Assert.That(categoryIds).Count().IsEqualTo(2);
        await Assert.That(tagIds).IsEquivalentTo([keptTag.Id, addedTag.Id]);
        await Assert.That(tagIds).Count().IsEqualTo(2);
    }

    [Test]
    public async Task UpdateAsync_WithStaleDetachedConcurrencyStamp_Throws()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenant = CreateTenant("blog-stale-update");
        seedContext.Tenants.Add(tenant);
        await seedContext.SaveChangesAsync();

        var blog = Blog.Create(tenant.Id, "Original", "original", "body");
        seedContext.Blogs.Add(blog);
        await seedContext.SaveChangesAsync();
        seedContext.ChangeTracker.Clear();

        var repository = new BlogRepository(seedContext);
        var detached = await repository.GetByTenantAndIdAsync(tenant.Id, blog.Id, CancellationToken.None);
        var persisted = await seedContext.Blogs.FirstAsync(existing => existing.Id == blog.Id);
        persisted.ConcurrencyStamp = Guid.NewGuid();
        await seedContext.SaveChangesAsync();
        seedContext.ChangeTracker.Clear();

        detached!.Update("Updated", "updated", "updated body");

        await Assert.That(repository.UpdateAsync(detached, CancellationToken.None)).Throws<DbUpdateConcurrencyException>();
    }

    [Test]
    public async Task UpdateAsync_WithRepeatedDetachedUpdate_ForcesFreshUpdatedAt()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenant = CreateTenant("blog-updated-at");
        seedContext.Tenants.Add(tenant);
        await seedContext.SaveChangesAsync();

        var blog = Blog.Create(tenant.Id, "Original", "original", "body");
        seedContext.Blogs.Add(blog);
        await seedContext.SaveChangesAsync();
        seedContext.ChangeTracker.Clear();

        var repository = new BlogRepository(seedContext);
        var firstDetached = await repository.GetByTenantAndIdAsync(tenant.Id, blog.Id, CancellationToken.None);
        firstDetached!.Update("First", "first", "first body");
        await repository.UpdateAsync(firstDetached, CancellationToken.None);
        seedContext.ChangeTracker.Clear();

        var firstUpdatedAt = await seedContext.Blogs
            .AsNoTracking()
            .Where(existing => existing.Id == blog.Id)
            .Select(existing => existing.UpdatedAt)
            .SingleAsync();
        var secondDetached = await repository.GetByTenantAndIdAsync(tenant.Id, blog.Id, CancellationToken.None);
        secondDetached!.UpdatedAt = firstUpdatedAt!.Value.AddDays(-1);
        secondDetached.Update("Second", "second", "second body");

        await repository.UpdateAsync(secondDetached, CancellationToken.None);
        seedContext.ChangeTracker.Clear();

        var secondUpdatedAt = await seedContext.Blogs
            .AsNoTracking()
            .Where(existing => existing.Id == blog.Id)
            .Select(existing => existing.UpdatedAt)
            .SingleAsync();

        await Assert.That(secondUpdatedAt).IsNotNull();
        await Assert.That(secondUpdatedAt).IsNotEqualTo(secondDetached.UpdatedAt);
        await Assert.That(secondUpdatedAt!.Value).IsGreaterThan(secondDetached.UpdatedAt!.Value);
    }

    [Test]
    public async Task CompositeRelationships_KeepThumbnailCategoryAndTagTenantSafe()
    {
        await fixture.ResetAsync();
        await using var seedContext = fixture.CreateDbContext();
        var tenantA = CreateTenant("blog-rel-a");
        var tenantB = CreateTenant("blog-rel-b");
        seedContext.Tenants.AddRange(tenantA, tenantB);
        await seedContext.SaveChangesAsync();

        var category = CreateCategory(tenantA.Id, "relationship-category");
        var tag = CreateTag(tenantA.Id, "relationship-tag");
        var thumbnail = CreateStorageObject(tenantA.Id);
        seedContext.Categories.Add(category);
        seedContext.Tags.Add(tag);
        seedContext.StorageObjects.Add(thumbnail);
        await seedContext.SaveChangesAsync();

        var blog = Blog.Create(tenantA.Id, "Relationships", "relationships", "body", thumbnail.Id);
        blog.ReplaceCategories([category.Id]);
        blog.ReplaceTags([tag.Id]);
        seedContext.Blogs.Add(blog);
        await seedContext.SaveChangesAsync();

        var invalidCrossTenantThumbnail = Blog.Create(tenantB.Id, "Invalid", "invalid", "body", thumbnail.Id);
        seedContext.Blogs.Add(invalidCrossTenantThumbnail);

        await Assert.That(seedContext.SaveChangesAsync()).Throws<DbUpdateException>();

        seedContext.ChangeTracker.Clear();
        var repository = new BlogRepository(seedContext);
        var loaded = await repository.GetByTenantAndIdAsync(tenantA.Id, blog.Id, CancellationToken.None);

        await Assert.That(loaded).IsNotNull();
        await Assert.That(loaded!.ThumbnailStorageObjectId).IsEqualTo(thumbnail.Id);
        await Assert.That(loaded.CategoryLinks.Select(link => link.CategoryId)).IsEquivalentTo([category.Id]);
        await Assert.That(loaded.TagLinks.Select(link => link.TagId)).IsEquivalentTo([tag.Id]);
    }

    private static Tenant CreateTenant(string slugPrefix) => new()
    {
        Id = Guid.CreateVersion7(),
        FullName = $"Blog Repository {slugPrefix}",
        Slug = $"{slugPrefix}-{Guid.NewGuid().ToString("N")[..8]}",
        TenantStatusId = (int)TenantStatusEnum.Active,
        TenantStatus = null!
    };

    private static Category CreateCategory(Guid tenantId, string code) => new()
    {
        Id = Guid.CreateVersion7(),
        TenantId = tenantId,
        Tenant = null!,
        MasterCode = code,
        FullName = code,
        ConcurrencyStamp = Guid.CreateVersion7()
    };

    private static Tag CreateTag(Guid tenantId, string code) => new()
    {
        Id = Guid.CreateVersion7(),
        TenantId = tenantId,
        Tenant = null!,
        MasterCode = code,
        FullName = code
    };

    private static StorageObject CreateStorageObject(Guid tenantId)
    {
        var id = Guid.CreateVersion7();
        return new StorageObject
        {
            Id = id,
            TenantId = tenantId,
            Tenant = null!,
            FileTypeId = (int)FileTypeEnum.Image,
            FileType = null!,
            Uri = $"/storage/{id:N}.png",
            ObjectKey = $"tenants/{tenantId:N}/{id:N}.png",
            Provider = StorageProviders.Local,
            FullName = "blog-thumbnail.png",
            SafeDisplayName = "blog-thumbnail.png",
            Extension = ".png",
            ContentType = "image/png",
            Size = 100,
            Visibility = StorageObjectVisibilities.PublicImage,
            Purpose = StorageObjectPurposes.BlogImage,
            LifecycleState = StorageObjectLifecycleStates.Active,
            CreatedAt = new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc),
            ConcurrencyStamp = Guid.CreateVersion7()
        };
    }

    private sealed record TestTenantContext(Guid TenantId) : ITenantContext;
}
