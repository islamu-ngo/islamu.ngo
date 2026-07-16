// ABOUTME: EF Core configuration for the CustomPropertyProjectionDirtyScope entity.
// ABOUTME: Defines the pending-drain backlog schema for custom-property projection rebuild coordination.

using Explore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class CustomPropertyProjectionDirtyScopeConfiguration : IEntityTypeConfiguration<CustomPropertyProjectionDirtyScope>
{
    public void Configure(EntityTypeBuilder<CustomPropertyProjectionDirtyScope> builder)
    {
        builder.Property(d => d.ProjectionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.ProjectionVersion);

        builder.Property(d => d.ScopeType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Reason)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(d => d.DrainedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasKey(d => d.Id)
            .HasName("pk_custom_property_projection_dirty_scope");

        builder.HasIndex("TenantId")
            .HasDatabaseName("ix_custom_property_projection_dirty_scope_tenant_id");

        builder.HasIndex("ProjectionName", "ProjectionVersion", "TenantId")
            .HasDatabaseName("ix_dirty_scope_pending")
            .HasFilter("drained_at IS NULL");

        builder.HasIndex("ProjectionName", "ProjectionVersion", "TenantId", "ScopeType", "ScopeId", "DefinitionId")
            .IsUnique()
            .HasDatabaseName("ix_dirty_scope_unique");

        builder.HasOne(d => d.Tenant)
            .WithMany()
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_custom_property_projection_dirty_scope_tenants_tenant_id");

        builder.ToTable("custom_property_projection_dirty_scope", (string)null);
    }
}
