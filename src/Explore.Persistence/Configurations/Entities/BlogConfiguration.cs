// ABOUTME: EF Core mapping for tenant-scoped blog aggregate publication records.
// ABOUTME: Enforces UUIDv7 keys, tenant-safe thumbnail FK, slug uniqueness, audit, soft-delete, and concurrency metadata.

using Explore.Domain;
using Explore.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.Property(e => e.Id).HasValueGenerator<GuidVersion7ValueGenerator>();
        builder.HasAlternateKey(e => new { e.TenantId, e.Id })
            .HasName("ak_blogs_tenant_id_id");

        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ContentMarkdown).HasColumnType("text").IsRequired();
        builder.Property(e => e.ConcurrencyStamp).IsConcurrencyToken();

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ThumbnailStorageObject)
            .WithMany()
            .HasForeignKey(e => new { e.TenantId, e.ThumbnailStorageObjectId })
            .HasPrincipalKey(e => new { e.TenantId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.Slug })
            .IsUnique()
            .HasDatabaseName("ux_blogs_tenant_slug");

        builder.HasIndex(e => new { e.TenantId, e.PublishedAtUtc })
            .HasDatabaseName("ix_blogs_tenant_published_at_utc");
    }
}
