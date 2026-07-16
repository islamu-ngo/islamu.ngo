// ABOUTME: EF Core configuration for the CustomPropertyProjectionStatus entity.
// ABOUTME: Defines the composite-key rebuild-status schema for custom-property projection observability.

using Explore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class CustomPropertyProjectionStatusConfiguration : IEntityTypeConfiguration<CustomPropertyProjectionStatus>
{
    public void Configure(EntityTypeBuilder<CustomPropertyProjectionStatus> builder)
    {
        builder.Property(s => s.ProjectionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ProjectionVersion);

        builder.Property(s => s.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.LastCheckpoint)
            .HasMaxLength(200);

        builder.Property(s => s.LastErrorMessage)
            .HasMaxLength(2000);

        builder.Property(s => s.RowsProcessed);

        builder.Property(s => s.RowsFailed);

        builder.HasKey("ProjectionName", "ProjectionVersion", "TenantId")
            .HasName("pk_custom_property_projection_status");

        builder.HasIndex("TenantId")
            .HasDatabaseName("ix_custom_property_projection_status_tenant_id");

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_custom_property_projection_status_tenants_tenant_id");

        builder.ToTable("custom_property_projection_status", (string)null);
    }
}
