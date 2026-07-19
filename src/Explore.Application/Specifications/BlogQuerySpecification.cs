// ABOUTME: Immutable application-layer query specification for Blog filtering and sorting.
// ABOUTME: Composes expression filters without referencing EF Core or leaking IQueryable from repositories.

using System.Linq.Expressions;
using Explore.Domain;

namespace Explore.Application.Specifications;

public sealed class BlogQuerySpecification : IQuerySpecification<Blog>
{
    private BlogQuerySpecification(
        IReadOnlyList<IFilterSpecification<Blog>> filters,
        ISortSpecification<Blog>? sort,
        bool sortDescending)
    {
        Filters = filters;
        Sort = sort;
        SortDescending = sortDescending;
    }

    public static BlogQuerySpecification Empty { get; } = new([], null, false);

    public IReadOnlyList<IFilterSpecification<Blog>> Filters { get; }
    public ISortSpecification<Blog>? Sort { get; }
    public bool SortDescending { get; }
    public bool HasFilters => Filters.Count > 0;
    public bool HasSort => Sort is not null;

    public BlogQuerySpecification ForTenant(Guid tenantId)
    {
        EnsureNotEmpty(tenantId, nameof(tenantId));
        return (BlogQuerySpecification)And(new BlogFilter(blog => blog.TenantId == tenantId));
    }

    public BlogQuerySpecification PublishedAt(DateTime nowUtc)
    {
        EnsureUtc(nowUtc, nameof(nowUtc));
        return (BlogQuerySpecification)And(new BlogFilter(blog => !blog.IsDeleted && blog.PublishedAtUtc != null && blog.PublishedAtUtc <= nowUtc));
    }

    public BlogQuerySpecification WithCategory(Guid categoryId)
    {
        EnsureNotEmpty(categoryId, nameof(categoryId));
        return (BlogQuerySpecification)And(new BlogFilter(blog => blog.CategoryLinks.Any(link => link.CategoryId == categoryId)));
    }

    public BlogQuerySpecification WithTag(Guid tagId)
    {
        EnsureNotEmpty(tagId, nameof(tagId));
        return (BlogQuerySpecification)And(new BlogFilter(blog => blog.TagLinks.Any(link => link.TagId == tagId)));
    }

    public BlogQuerySpecification SearchTitle(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return this;
        }

        string normalized = searchTerm.Trim();
        return (BlogQuerySpecification)And(new BlogFilter(blog => blog.Title.Contains(normalized)));
    }

    public BlogQuerySpecification SortByPublishedAtDescending()
    {
        return (BlogQuerySpecification)SortByDescending(new BlogSort(blog => blog.PublishedAtUtc ?? DateTime.MinValue));
    }

    public BlogQuerySpecification SortByTitle()
    {
        return (BlogQuerySpecification)SortBy(new BlogSort(blog => blog.Title));
    }

    public IQuerySpecification<Blog> And(IFilterSpecification<Blog> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return new BlogQuerySpecification([.. Filters, filter], Sort, SortDescending);
    }

    public IQuerySpecification<Blog> SortBy(ISortSpecification<Blog> sort)
    {
        ArgumentNullException.ThrowIfNull(sort);
        return new BlogQuerySpecification(Filters, sort, false);
    }

    public IQuerySpecification<Blog> SortByDescending(ISortSpecification<Blog> sort)
    {
        ArgumentNullException.ThrowIfNull(sort);
        return new BlogQuerySpecification(Filters, sort, true);
    }

    public IQueryable<Blog> Apply(IQueryable<Blog> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        foreach (IFilterSpecification<Blog> filter in Filters)
        {
            query = query.Where(filter.Predicate);
        }

        if (Sort is null)
        {
            return query;
        }

        return SortDescending
            ? query.OrderByDescending(Sort.KeySelector)
            : query.OrderBy(Sort.KeySelector);
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required.", parameterName);
        }
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }

    private sealed record BlogFilter(Expression<Func<Blog, bool>> Predicate) : IFilterSpecification<Blog>;

    private sealed record BlogSort(Expression<Func<Blog, object>> KeySelector) : ISortSpecification<Blog>;
}
