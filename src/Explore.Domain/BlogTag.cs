// ABOUTME: Tenant-scoped join entity connecting blog posts to existing tag records.
// ABOUTME: Uses UUIDv7 identity so tag replacement can stay entity-first and repository-friendly.

using Explore.Domain.Interfaces;

namespace Explore.Domain;

public class BlogTag : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; private set; } = null!;
    public Guid BlogId { get; private set; }
    public Blog Blog { get; private set; } = null!;
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;

    public static BlogTag Create(Guid tenantId, Guid blogId, Guid tagId)
    {
        EnsureNotEmpty(tenantId, nameof(tenantId));
        EnsureNotEmpty(blogId, nameof(blogId));
        EnsureNotEmpty(tagId, nameof(tagId));

        return new BlogTag
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            BlogId = blogId,
            TagId = tagId
        };
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required.", parameterName);
        }
    }
}
