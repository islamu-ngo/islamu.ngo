// ABOUTME: Tenant-scoped join entity connecting blog posts to existing category records.
// ABOUTME: Uses UUIDv7 identity so category replacement can stay entity-first and repository-friendly.

using Explore.Domain.Interfaces;

namespace Explore.Domain;

public class BlogCategory : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; private set; } = null!;
    public Guid BlogId { get; private set; }
    public Blog Blog { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public static BlogCategory Create(Guid tenantId, Guid blogId, Guid categoryId)
    {
        EnsureNotEmpty(tenantId, nameof(tenantId));
        EnsureNotEmpty(blogId, nameof(blogId));
        EnsureNotEmpty(categoryId, nameof(categoryId));

        return new BlogCategory
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            BlogId = blogId,
            CategoryId = categoryId
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
