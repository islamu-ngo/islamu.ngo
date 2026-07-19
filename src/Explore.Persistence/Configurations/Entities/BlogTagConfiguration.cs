// ABOUTME: EF Core mapping for tenant-scoped blog-to-tag join rows.
// ABOUTME: Uses UUIDv7 IDs and composite tenant FKs to prevent cross-tenant blog/tag links.

using Explore.Domain;
using Explore.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTag>
{
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.Property(e => e.Id).HasValueGenerator<GuidVersion7ValueGenerator>();

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Blog)
            .WithMany(e => e.TagLinks)
            .HasForeignKey(e => new { e.TenantId, e.BlogId })
            .HasPrincipalKey(e => new { e.TenantId, e.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Tag)
            .WithMany()
            .HasForeignKey(e => new { e.TenantId, e.TagId })
            .HasPrincipalKey(e => new { e.TenantId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.BlogId, e.TagId })
            .IsUnique()
            .HasDatabaseName("ux_blog_tags_tenant_blog_tag");
    }
}
