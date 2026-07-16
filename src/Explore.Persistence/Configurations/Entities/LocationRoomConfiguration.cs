// ABOUTME: EF Core configuration for the LocationRoom entity.
// ABOUTME: Enforces tenant-scoped alternate keys, unique room names per location, and non-negative capacity check.

using Explore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.Persistence.Configurations.Entities;

public class LocationRoomConfiguration : IEntityTypeConfiguration<LocationRoom>
{
    public void Configure(EntityTypeBuilder<LocationRoom> builder)
    {
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Slug)
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.HasKey(r => r.Id)
            .HasName("pk_location_rooms");

        builder.HasAlternateKey("TenantId", "Id")
            .HasName("ak_location_rooms_tenant_id_id");

        builder.HasAlternateKey("TenantId", "LocationId", "Id")
            .HasName("ak_location_rooms_tenant_id_location_id_id");

        builder.HasIndex("TenantId", "LocationId", "Name")
            .IsUnique()
            .HasDatabaseName("ix_location_rooms_tenant_location_name");

        builder.HasIndex("TenantId", "LocationId", "SortOrder")
            .HasDatabaseName("ix_location_rooms_tenant_location_sort");

        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_location_rooms_tenants_tenant_id");

        builder.HasOne(r => r.Location)
            .WithMany()
            .HasForeignKey(r => new { r.TenantId, r.LocationId })
            .HasPrincipalKey(l => new { l.TenantId, l.Id })
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_location_rooms_locations_tenant_id_location_id");

        builder.ToTable("location_rooms", null, t =>
        {
            t.HasCheckConstraint("CK_LocationRoom_NonNegativeCapacity", "capacity IS NULL OR capacity >= 0");
        });
    }
}
