// ABOUTME: EF Core repository for tenant-scoped Blog aggregate persistence and query specification reads.
// ABOUTME: Returns entities only, keeps reads no-tracking, and preserves tenant isolation for soft-delete slug checks.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Specifications;
using Explore.Domain;
using Explore.Persistence.QueryFilters;
using Microsoft.EntityFrameworkCore;

namespace Explore.Persistence.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly ExploreDbContext _dbContext;

    public BlogRepository(ExploreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Blog> CreateAsync(Blog blog, CancellationToken cancellationToken)
    {
        await _dbContext.Blogs.AddAsync(blog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return blog;
    }

    public async Task<Blog> UpdateAsync(Blog blog, CancellationToken cancellationToken)
    {
        var persisted = await _dbContext.Blogs
            .Include(existing => existing.CategoryLinks)
            .Include(existing => existing.TagLinks)
            .FirstAsync(existing => existing.TenantId == blog.TenantId && existing.Id == blog.Id, cancellationToken);
        var persistedId = persisted.Id;
        var persistedTenantId = persisted.TenantId;

        if (persisted.ConcurrencyStamp != blog.ConcurrencyStamp)
        {
            throw new DbUpdateConcurrencyException("The blog was modified by another request.");
        }

        _dbContext.Entry(persisted).CurrentValues.SetValues(blog);
        _dbContext.Entry(persisted).Property(existing => existing.Id).CurrentValue = persistedId;
        _dbContext.Entry(persisted).Property(existing => existing.TenantId).CurrentValue = persistedTenantId;
        persisted.UpdatedAt = null;

        ReconcileCategoryLinks(persisted, blog.CategoryLinks.Select(link => link.CategoryId));
        ReconcileTagLinks(persisted, blog.TagLinks.Select(link => link.TagId));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return persisted;
    }

    public async Task<Blog?> GetByTenantAndIdAsync(
        Guid tenantId,
        Guid blogId,
        CancellationToken cancellationToken)
    {
        return await BlogDetailsQuery()
            .FirstOrDefaultAsync(blog => blog.TenantId == tenantId && blog.Id == blogId, cancellationToken);
    }

    public async Task<Blog?> GetByTenantAndSlugAsync(
        Guid tenantId,
        string slug,
        CancellationToken cancellationToken)
    {
        return await BlogDetailsQuery()
            .FirstOrDefaultAsync(blog => blog.TenantId == tenantId && blog.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Blog>> ListAsync(
        BlogQuerySpecification specification,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ValidateBounds(skip, take);

        return await specification.Apply(BlogDetailsQuery())
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        BlogQuerySpecification specification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await specification.Apply(_dbContext.Blogs.AsNoTracking())
            .CountAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        Guid tenantId,
        string slug,
        Guid? excludingBlogId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Blogs
            .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
            .AsNoTracking()
            .AnyAsync(blog =>
                blog.TenantId == tenantId &&
                blog.Slug == slug &&
                (!excludingBlogId.HasValue || blog.Id != excludingBlogId.Value),
                cancellationToken);
    }

    private IQueryable<Blog> BlogDetailsQuery()
    {
        return _dbContext.Blogs
            .AsNoTracking()
            .AsSplitQuery()
            .Include(blog => blog.CategoryLinks)
                .ThenInclude(link => link.Category)
            .Include(blog => blog.TagLinks)
                .ThenInclude(link => link.Tag)
            .Include(blog => blog.ThumbnailStorageObject);
    }

    private static void ValidateBounds(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be zero or greater.");
        }

        if (take is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be between 1 and 200.");
        }
    }

    private void ReconcileCategoryLinks(Blog persisted, IEnumerable<Guid> categoryIds)
    {
        var desired = categoryIds.ToHashSet();

        foreach (var stale in persisted.CategoryLinks.Where(link => !desired.Contains(link.CategoryId)).ToList())
        {
            persisted.CategoryLinks.Remove(stale);
        }

        var existing = persisted.CategoryLinks.Select(link => link.CategoryId).ToHashSet();
        foreach (var categoryId in desired.Except(existing))
        {
            var link = BlogCategory.Create(persisted.TenantId, persisted.Id, categoryId);
            persisted.CategoryLinks.Add(link);
            _dbContext.Entry(link).State = EntityState.Added;
        }
    }

    private void ReconcileTagLinks(Blog persisted, IEnumerable<Guid> tagIds)
    {
        var desired = tagIds.ToHashSet();

        foreach (var stale in persisted.TagLinks.Where(link => !desired.Contains(link.TagId)).ToList())
        {
            persisted.TagLinks.Remove(stale);
        }

        var existing = persisted.TagLinks.Select(link => link.TagId).ToHashSet();
        foreach (var tagId in desired.Except(existing))
        {
            var link = BlogTag.Create(persisted.TenantId, persisted.Id, tagId);
            persisted.TagLinks.Add(link);
            _dbContext.Entry(link).State = EntityState.Added;
        }
    }
}
