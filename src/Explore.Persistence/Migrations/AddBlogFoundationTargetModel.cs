// ABOUTME: Freezes the EF Core target model after adding tenant-safe Blog foundation tables.
// ABOUTME: Layers Blog metadata over the immutable Phase 10 post-Event product-surface model.

using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Explore.Persistence.Migrations
{
    internal static class AddBlogFoundationTargetModel
    {
        internal static void Apply(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            RemoveEventProductSurfaceTargetModel.Apply(modelBuilder);

            modelBuilder.Entity("Explore.Domain.StorageObject", b =>
                {
                    b.ToTable("storage_objects", null, t =>
                        {
                            t.HasCheckConstraint("ck_storage_objects_lifecycle_state", "lifecycle_state IN ('pending', 'active', 'quarantined', 'delete_requested', 'deleted')");
                            t.HasCheckConstraint("ck_storage_objects_provider", "provider IN ('local', 's3_compatible', 'legacy_external')");
                            t.HasCheckConstraint("ck_storage_objects_purpose", "purpose IN ('legacy_image', 'profile_image', 'blog_image', 'attachment', 'document', 'system_asset')");
                            t.HasCheckConstraint("ck_storage_objects_size_nonnegative", "size >= 0");
                            t.HasCheckConstraint("ck_storage_objects_visibility", "visibility IN ('public_image', 'authenticated_tenant', 'private_owner')");
                        });
                });

            modelBuilder.Entity("Explore.Domain.StorageUploadSession", b =>
                {
                    b.ToTable("storage_upload_sessions", null, t =>
                        {
                            t.HasCheckConstraint("ck_storage_upload_sessions_expected_size_nonnegative", "expected_size_bytes >= 0");
                            t.HasCheckConstraint("ck_storage_upload_sessions_policy_max_upload_bytes_nonnegative", "policy_max_upload_bytes >= 0");
                            t.HasCheckConstraint("ck_storage_upload_sessions_provider", "provider IN ('local', 's3_compatible', 'legacy_external')");
                            t.HasCheckConstraint("ck_storage_upload_sessions_purpose", "purpose IN ('legacy_image', 'profile_image', 'blog_image', 'attachment', 'document', 'system_asset')");
                            t.HasCheckConstraint("ck_storage_upload_sessions_reserved_bytes_nonnegative", "reserved_bytes >= 0");
                            t.HasCheckConstraint("ck_storage_upload_sessions_route_key", "route_key IN ('images', 'documents', 'general')");
                            t.HasCheckConstraint("ck_storage_upload_sessions_status", "status IN ('reserved', 'uploading', 'finalized', 'canceled', 'failed', 'expired')");
                            t.HasCheckConstraint("ck_storage_upload_sessions_visibility", "visibility IN ('public_image', 'authenticated_tenant', 'private_owner')");
                        });
                });

            modelBuilder.Entity("Explore.Domain.Blog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("uuidv7()");

                    b.Property<Guid>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("ContentMarkdown")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content_markdown");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<DateTime?>("PublishedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("published_at_utc");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("slug");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.Property<Guid?>("ThumbnailStorageObjectId")
                        .HasColumnType("uuid")
                        .HasColumnName("thumbnail_storage_object_id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("title");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("updated_by");

                    b.HasKey("Id")
                        .HasName("pk_blogs");

                    b.HasAlternateKey("TenantId", "Id")
                        .HasName("ak_blogs_tenant_id_id");

                    b.HasIndex("TenantId", "ThumbnailStorageObjectId")
                        .HasDatabaseName("ix_blogs_tenant_id_thumbnail_storage_object_id");

                    b.HasIndex("TenantId", "PublishedAtUtc")
                        .HasDatabaseName("ix_blogs_tenant_published_at_utc");

                    b.HasIndex("TenantId", "Slug")
                        .IsUnique()
                        .HasDatabaseName("ux_blogs_tenant_slug");

                    b.ToTable("blogs", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.BlogCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("uuidv7()");

                    b.Property<Guid>("BlogId")
                        .HasColumnType("uuid")
                        .HasColumnName("blog_id");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid")
                        .HasColumnName("category_id");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.HasKey("Id")
                        .HasName("pk_blog_categories");

                    b.HasIndex("TenantId", "CategoryId")
                        .HasDatabaseName("ix_blog_categories_tenant_id_category_id");

                    b.HasIndex("TenantId", "BlogId", "CategoryId")
                        .IsUnique()
                        .HasDatabaseName("ux_blog_categories_tenant_blog_category");

                    b.ToTable("blog_categories", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.BlogTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("uuidv7()");

                    b.Property<Guid>("BlogId")
                        .HasColumnType("uuid")
                        .HasColumnName("blog_id");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid")
                        .HasColumnName("tag_id");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid")
                        .HasColumnName("tenant_id");

                    b.HasKey("Id")
                        .HasName("pk_blog_tags");

                    b.HasIndex("TenantId", "TagId")
                        .HasDatabaseName("ix_blog_tags_tenant_id_tag_id");

                    b.HasIndex("TenantId", "BlogId", "TagId")
                        .IsUnique()
                        .HasDatabaseName("ux_blog_tags_tenant_blog_tag");

                    b.ToTable("blog_tags", (string)null);
                });

            modelBuilder.Entity("Explore.Domain.Blog", b =>
                {
                    b.HasOne("Explore.Domain.StorageObject", "ThumbnailStorageObject")
                        .WithMany()
                        .HasForeignKey("TenantId", "ThumbnailStorageObjectId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_blogs_storage_objects_tenant_id_thumbnail_storage_object_id");

                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_blogs_tenants_tenant_id");

                    b.Navigation("Tenant");
                    b.Navigation("ThumbnailStorageObject");
                });

            modelBuilder.Entity("Explore.Domain.BlogCategory", b =>
                {
                    b.HasOne("Explore.Domain.Blog", "Blog")
                        .WithMany("CategoryLinks")
                        .HasForeignKey("TenantId", "BlogId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_blog_categories_blogs_tenant_id_blog_id");

                    b.HasOne("Explore.Domain.Category", "Category")
                        .WithMany()
                        .HasForeignKey("TenantId", "CategoryId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_blog_categories_categories_tenant_id_category_id");

                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_blog_categories_tenants_tenant_id");

                    b.Navigation("Blog");
                    b.Navigation("Category");
                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Explore.Domain.BlogTag", b =>
                {
                    b.HasOne("Explore.Domain.Blog", "Blog")
                        .WithMany("TagLinks")
                        .HasForeignKey("TenantId", "BlogId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_blog_tags_blogs_tenant_id_blog_id");

                    b.HasOne("Explore.Domain.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TenantId", "TagId")
                        .HasPrincipalKey("TenantId", "Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_blog_tags_tags_tenant_id_tag_id");

                    b.HasOne("Explore.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_blog_tags_tenants_tenant_id");

                    b.Navigation("Blog");
                    b.Navigation("Tag");
                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Explore.Domain.Blog", b =>
                {
                    b.Navigation("CategoryLinks");
                    b.Navigation("TagLinks");
                });
#pragma warning restore 612, 618
        }
    }
}
