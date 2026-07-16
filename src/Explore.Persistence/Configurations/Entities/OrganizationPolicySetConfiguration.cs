// ABOUTME: EF Core configuration for the retained organization-level governance policy record.
// ABOUTME: Maps identity, tenancy, audit metadata, concurrency, and organization uniqueness.

using Explore.Domain.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class OrganizationPolicySetConfiguration : IEntityTypeConfiguration<OrganizationPolicySet>
{
    public void Configure(EntityTypeBuilder<OrganizationPolicySet> builder)
    {
        builder.ToTable("organization_policy_sets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.OrganizationId).IsUnique();
    }
}
