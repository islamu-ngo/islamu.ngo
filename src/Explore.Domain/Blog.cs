// ABOUTME: Tenant-scoped blog aggregate that stores raw Markdown and explicit publication lifecycle state.
// ABOUTME: Owns category/tag join replacement while preserving audit, soft-delete, and concurrency metadata.

using Explore.Domain.Interfaces;

namespace Explore.Domain;

public class Blog : ITenantEntity, IAuditableEntity, ISoftDeletable, IConcurrencyAware
{
    private const int MaxSlugLength = 200;

    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string ContentMarkdown { get; private set; } = string.Empty;
    public Guid? ThumbnailStorageObjectId { get; private set; }
    public StorageObject? ThumbnailStorageObject { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public ICollection<BlogCategory> CategoryLinks { get; set; } = [];
    public ICollection<BlogTag> TagLinks { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public Guid ConcurrencyStamp { get; set; }

    public static Blog Create(
        Guid tenantId,
        string title,
        string slug,
        string contentMarkdown,
        Guid? thumbnailStorageObjectId = null)
    {
        EnsureNotEmpty(tenantId, nameof(tenantId));
        EnsureOptionalNotEmpty(thumbnailStorageObjectId, nameof(thumbnailStorageObjectId));

        return new Blog
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Title = NormalizeRequired(title, nameof(title)),
            Slug = NormalizeCanonicalSlug(slug, nameof(slug)),
            ContentMarkdown = contentMarkdown ?? string.Empty,
            ThumbnailStorageObjectId = thumbnailStorageObjectId
        };
    }

    public void Update(
        string title,
        string slug,
        string contentMarkdown,
        Guid? thumbnailStorageObjectId = null)
    {
        EnsureNotDeleted();
        EnsureOptionalNotEmpty(thumbnailStorageObjectId, nameof(thumbnailStorageObjectId));

        Title = NormalizeRequired(title, nameof(title));
        Slug = NormalizeCanonicalSlug(slug, nameof(slug));
        ContentMarkdown = contentMarkdown ?? string.Empty;
        ThumbnailStorageObjectId = thumbnailStorageObjectId;
    }

    public void Publish(DateTime publishedAtUtc)
    {
        EnsureNotDeleted();
        EnsureUtc(publishedAtUtc, nameof(publishedAtUtc));

        if (PublishedAtUtc is not null)
        {
            throw new InvalidOperationException("Only unpublished blog posts can be published.");
        }

        PublishedAtUtc = publishedAtUtc;
    }

    public void Unpublish()
    {
        EnsureNotDeleted();

        if (PublishedAtUtc is null)
        {
            throw new InvalidOperationException("Only published blog posts can be unpublished.");
        }

        PublishedAtUtc = null;
    }

    public void MarkDeleted(Guid? userId, DateTime deletedAtUtc)
    {
        EnsureOptionalNotEmpty(userId, nameof(userId));
        EnsureUtc(deletedAtUtc, nameof(deletedAtUtc));

        if (IsDeleted)
        {
            return;
        }

        PublishedAtUtc = null;
        IsDeleted = true;
        DeletedAt = deletedAtUtc;
        DeletedBy = userId;
    }

    public void ReplaceCategories(IReadOnlyCollection<Guid> categoryIds)
    {
        EnsureNotDeleted();

        CategoryLinks = Deduplicate(categoryIds, nameof(categoryIds))
            .Select(categoryId => BlogCategory.Create(TenantId, Id, categoryId))
            .ToList();
    }

    public void ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        EnsureNotDeleted();

        TagLinks = Deduplicate(tagIds, nameof(tagIds))
            .Select(tagId => BlogTag.Create(TenantId, Id, tagId))
            .ToList();
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted blog posts cannot be changed.");
        }
    }

    private static IReadOnlyList<Guid> Deduplicate(IReadOnlyCollection<Guid> ids, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(ids);
        return ids.Select(id =>
            {
                EnsureNotEmpty(id, parameterName);
                return id;
            })
            .Distinct()
            .ToList();
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeCanonicalSlug(string value, string parameterName)
    {
        var slug = NormalizeRequired(value, parameterName);
        if (slug.Length > MaxSlugLength)
        {
            throw new ArgumentException("Slug must be 200 characters or fewer.", parameterName);
        }

        var previousWasHyphen = false;
        for (var index = 0; index < slug.Length; index++)
        {
            var current = slug[index];
            if (current == '-')
            {
                if (index == 0 || index == slug.Length - 1 || previousWasHyphen)
                {
                    throw new ArgumentException("Slug must not start, end, or repeat hyphens.", parameterName);
                }

                previousWasHyphen = true;
                continue;
            }

            if (!char.IsLetterOrDigit(current) || current != char.ToLowerInvariant(current))
            {
                throw new ArgumentException("Slug must contain lowercase Unicode letters, digits, and single hyphens only.", parameterName);
            }

            previousWasHyphen = false;
        }

        return slug;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required.", parameterName);
        }
    }

    private static void EnsureOptionalNotEmpty(Guid? value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required when provided.", parameterName);
        }
    }
}
