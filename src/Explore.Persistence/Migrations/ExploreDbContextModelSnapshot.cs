// ABOUTME: Captures the retained persistence model after Event product-surface removal.
// ABOUTME: Stores conventional generated EF Core snapshot metadata without runtime model construction.

using System;
using Explore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Explore.Persistence.Migrations
{
    [DbContext(typeof(ExploreDbContext))]
    partial class ExploreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
            => BuildRetainedModel(modelBuilder);

        internal static void BuildRetainedModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "btree_gist");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Explore.Domain.AccountAuthorityKindLookup", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("description");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("full_name");

                    b.Property<string>("MasterCode")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("master_code");

                    b.HasKey("Id")
                        .HasName("pk_account_authority_kinds");

                    b.HasIndex("MasterCode")
                        .IsUnique()
                        .HasDatabaseName("ux_account_authority_kinds_master_code");

                    b.ToTable("account_authority_kinds", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.Actor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("ActorTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("actor_type_id");

                    b.Property<string>("BackgroundColor")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("background_color");

                    b.Property<string>("BackgroundEffect")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("background_effect");

                    b.Property<Guid?>("BackgroundImageId")
                        .HasColumnType("uuid")
                        .HasColumnName("background_image_id");

                    b.Property<string>("BannerColor")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("banner_color");

                    b.Property<Guid?>("BannerPictureId")
                        .HasColumnType("uuid")
                        .HasColumnName("banner_picture_id");

                    b.Property<Guid>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasColumnName("concurrency_stamp");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("created_by");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<Guid?>("DeletedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("deleted_by");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("description");

                    b.Property<int?>("DidCustodyTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("did_custody_type_id");

                    b.Property<Guid?>("GroupId")
                        .HasColumnType("uuid")
                        .HasColumnName("group_id");

                    b.Property<DateTime?>("IndexedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("indexed_at");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<Guid?>("OrganizationId")
                        .HasColumnType("uuid")
                        .HasColumnName("organization_id");

                    b.Property<string>("PdsHost")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("pds_host");

                    b.Property<string>("ProfilePictureCid")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("profile_picture_cid");

                    b.Property<Guid?>("ProfilePictureId")
                        .HasColumnType("uuid")
                        .HasColumnName("profile_picture_id");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("updated_by");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_actors");

                    b.HasAlternateKey("TenantId", "Id")
                        .HasName("ak_actors_tenant_id_id");

                    b.HasIndex("ActorTypeId")
                        .HasDatabaseName("ix_actors_actor_type_id");

                    b.HasIndex("BackgroundImageId")
                        .HasDatabaseName("ix_actors_background_image_id");

                    b.HasIndex("BannerPictureId")
                        .HasDatabaseName("ix_actors_banner_picture_id");

                    b.HasIndex("DidCustodyTypeId")
                        .HasDatabaseName("ix_actors_did_custody_type_id");

                    b.HasIndex("GroupId")
                        .IsUnique()
                        .HasDatabaseName("ix_actors_group_id")
                        .HasFilter("group_id IS NOT NULL");

                    b.HasIndex("OrganizationId")
                        .IsUnique()
                        .HasDatabaseName("ix_actors_organization_id")
                        .HasFilter("organization_id IS NOT NULL");

                    b.HasIndex("ProfilePictureId")
                        .HasDatabaseName("ix_actors_profile_picture_id");

                    b.HasIndex("UserId", "TenantId")
                        .IsUnique()
                        .HasDatabaseName("ix_actors_user_id_tenant_id")
                        .HasFilter("user_id IS NOT NULL");

                    b.ToTable("actors", null, t =>
                        {
                            t.HasCheckConstraint("CK_Actor_UserOrOrganization", "(user_id IS NOT NULL AND organization_id IS NULL AND group_id IS NULL) OR (user_id IS NULL AND organization_id IS NOT NULL AND group_id IS NULL) OR (user_id IS NULL AND organization_id IS NULL AND group_id IS NOT NULL) OR (user_id IS NULL AND organization_id IS NULL AND group_id IS NULL)");
                        });
                });

            modelBuilder.Entity("Explore.Domain.LocationRoom", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int?>("Capacity")
                        .HasColumnType("integer")
                        .HasColumnName("capacity");

                    b.Property<Guid>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasColumnName("concurrency_stamp");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("created_by");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<Guid?>("DeletedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("deleted_by");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("description");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("name");

                    b.Property<string>("Slug")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("slug");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer")
                        .HasColumnName("sort_order");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("updated_by");

                    b.HasKey("Id")
                        .HasName("pk_location_rooms");

                    b.HasAlternateKey("TenantId", "Id")
                        .HasName("ak_location_rooms_tenant_id_id");

                    b.HasAlternateKey("TenantId", "LocationId", "Id")
                        .HasName("ak_location_rooms_tenant_id_location_id_id");

                    b.HasIndex("TenantId", "LocationId", "Name")
                        .IsUnique()
                        .HasDatabaseName("ix_location_rooms_tenant_location_name");

                    b.HasIndex("TenantId", "LocationId", "SortOrder")
                        .HasDatabaseName("ix_location_rooms_tenant_location_sort");

                    b.ToTable("location_rooms", null, t =>
                        {
                            t.HasCheckConstraint("CK_LocationRoom_NonNegativeCapacity", "capacity IS NULL OR capacity >= 0");
                        });
                });

            modelBuilder.Entity("Explore.Domain.LocationRoom", b =>
                {
                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_location_rooms_tenants_tenant_id");

                    b.HasOne("Explore.Domain.Location", "Location")
                        .WithMany()
                        .HasForeignKey("TenantId", "LocationId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_location_rooms_locations_tenant_id_location_id");

                    b.Navigation("Location");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Explore.Domain.CustomPropertyProjectionDirtyScope", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid?>("DefinitionId")
                        .HasColumnType("uuid")
                        .HasColumnName("definition_id");

                    b.Property<DateTimeOffset?>("DrainedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("drained_at");

                    b.Property<string>("ProjectionName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("projection_name");

                    b.Property<int>("ProjectionVersion")
                        .HasColumnType("integer")
                        .HasColumnName("projection_version");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("reason");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("uuid")
                        .HasColumnName("scope_id");

                    b.Property<string>("ScopeType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("scope_type");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.HasKey("Id")
                        .HasName("pk_custom_property_projection_dirty_scope");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("ix_custom_property_projection_dirty_scope_tenant_id");

                    b.HasIndex("ProjectionName", "ProjectionVersion", "TenantId")
                        .HasDatabaseName("ix_dirty_scope_pending")
                        .HasFilter("drained_at IS NULL");

                    b.HasIndex("ProjectionName", "ProjectionVersion", "TenantId", "ScopeType", "ScopeId", "DefinitionId")
                        .IsUnique()
                        .HasDatabaseName("ix_dirty_scope_unique");

                    b.ToTable("custom_property_projection_dirty_scope", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.CustomPropertyProjectionDirtyScope", b =>
                {
                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_custom_property_projection_dirty_scope_tenants_tenant_id");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Explore.Domain.CustomPropertyProjectionStatus", b =>
                {
                    b.Property<string>("ProjectionName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("projection_name");

                    b.Property<int>("ProjectionVersion")
                        .HasColumnType("integer")
                        .HasColumnName("projection_version");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<Guid>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("LastCheckpoint")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("last_checkpoint");

                    b.Property<string>("LastErrorMessage")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("last_error_message");

                    b.Property<DateTimeOffset?>("LastRebuildCompletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_rebuild_completed_at");

                    b.Property<DateTimeOffset?>("LastRebuildStartedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_rebuild_started_at");

                    b.Property<long>("RowsFailed")
                        .HasColumnType("bigint")
                        .HasColumnName("rows_failed");

                    b.Property<long>("RowsProcessed")
                        .HasColumnType("bigint")
                        .HasColumnName("rows_processed");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("state");

                    b.HasKey("ProjectionName", "ProjectionVersion", "TenantId")
                        .HasName("pk_custom_property_projection_status");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("ix_custom_property_projection_status_tenant_id");

                    b.ToTable("custom_property_projection_status", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.CustomPropertyProjectionStatus", b =>
                {
                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_custom_property_projection_status_tenants_tenant_id");

                    b.Navigation("Tenant");
                });

            modelBuilder.HasAnnotation("__retained_model_placeholder", "");
#pragma warning restore 612, 618
        }
    }
}
