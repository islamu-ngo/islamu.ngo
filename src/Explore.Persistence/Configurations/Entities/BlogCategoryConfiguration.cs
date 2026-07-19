// ABOUTME: EF Core mapping for tenant-scoped blog-to-category join rows.
// ABOUTME: Uses UUIDv7 IDs and composite tenant FKs to prevent cross-tenant blog/category links.

using Explore.Domain;
using Explore.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.Property(e => e.Id).HasValueGenerator<GuidVersion7ValueGenerator>();

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Blog)
            .WithMany(e => e.CategoryLinks)
            .HasForeignKey(e => new { e.TenantId, e.BlogId })
            .HasPrincipalKey(e => new { e.TenantId, e.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => new { e.TenantId, e.CategoryId })
            .HasPrincipalKey(e => new { e.TenantId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.BlogId, e.CategoryId })
            .IsUnique()
            .HasDatabaseName("ux_blog_categories_tenant_blog_category");
    }
}
