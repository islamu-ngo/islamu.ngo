// ABOUTME: Adds the tenant-safe Blog foundation schema and storage purpose support for blog images.
// ABOUTME: Keeps the migration reversible by restoring prior storage purpose constraints in Down.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.Persistence.Migrations;

public partial class AddBlogFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        AddBlogImageStoragePurpose(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "blogs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                content_markdown = table.Column<string>(type: "text", nullable: false),
                thumbnail_storage_object_id = table.Column<Guid>(type: "uuid", nullable: true),
                published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                concurrency_stamp = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_blogs", x => x.id);
                table.UniqueConstraint("ak_blogs_tenant_id_id", x => new { x.tenant_id, x.id });
                table.ForeignKey(
                    name: "fk_blogs_storage_objects_tenant_id_thumbnail_storage_object_id",
                    columns: x => new { x.tenant_id, x.thumbnail_storage_object_id },
                    principalTable: "storage_objects",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_blogs_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "blog_categories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                blog_id = table.Column<Guid>(type: "uuid", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_blog_categories", x => x.id);
                table.ForeignKey(
                    name: "fk_blog_categories_blogs_tenant_id_blog_id",
                    columns: x => new { x.tenant_id, x.blog_id },
                    principalTable: "blogs",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_blog_categories_categories_tenant_id_category_id",
                    columns: x => new { x.tenant_id, x.category_id },
                    principalTable: "categories",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_blog_categories_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "blog_tags",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                blog_id = table.Column<Guid>(type: "uuid", nullable: false),
                tag_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_blog_tags", x => x.id);
                table.ForeignKey(
                    name: "fk_blog_tags_blogs_tenant_id_blog_id",
                    columns: x => new { x.tenant_id, x.blog_id },
                    principalTable: "blogs",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_blog_tags_tags_tenant_id_tag_id",
                    columns: x => new { x.tenant_id, x.tag_id },
                    principalTable: "tags",
                    principalColumns: new[] { "tenant_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_blog_tags_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_blogs_tenant_id_thumbnail_storage_object_id",
            table: "blogs",
            columns: new[] { "tenant_id", "thumbnail_storage_object_id" });

        migrationBuilder.CreateIndex(
            name: "ix_blogs_tenant_published_at_utc",
            table: "blogs",
            columns: new[] { "tenant_id", "published_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ux_blogs_tenant_slug",
            table: "blogs",
            columns: new[] { "tenant_id", "slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_blog_categories_tenant_id_category_id",
            table: "blog_categories",
            columns: new[] { "tenant_id", "category_id" });

        migrationBuilder.CreateIndex(
            name: "ux_blog_categories_tenant_blog_category",
            table: "blog_categories",
            columns: new[] { "tenant_id", "blog_id", "category_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_blog_tags_tenant_id_tag_id",
            table: "blog_tags",
            columns: new[] { "tenant_id", "tag_id" });

        migrationBuilder.CreateIndex(
            name: "ux_blog_tags_tenant_blog_tag",
            table: "blog_tags",
            columns: new[] { "tenant_id", "blog_id", "tag_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("blog_categories");
        migrationBuilder.DropTable("blog_tags");
        migrationBuilder.DropTable("blogs");

        DeleteBlogImageStorageRows(migrationBuilder);
        RemoveBlogImageStoragePurpose(migrationBuilder);
    }

    private static void AddBlogImageStoragePurpose(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE storage_objects DROP CONSTRAINT IF EXISTS ck_storage_objects_purpose;
            ALTER TABLE storage_objects ADD CONSTRAINT ck_storage_objects_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'blog_image', 'attachment', 'document', 'system_asset'));
            ALTER TABLE storage_upload_sessions DROP CONSTRAINT IF EXISTS ck_storage_upload_sessions_purpose;
            ALTER TABLE storage_upload_sessions ADD CONSTRAINT ck_storage_upload_sessions_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'blog_image', 'attachment', 'document', 'system_asset'));
            """);
    }

    private static void RemoveBlogImageStoragePurpose(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE storage_objects DROP CONSTRAINT IF EXISTS ck_storage_objects_purpose;
            ALTER TABLE storage_objects ADD CONSTRAINT ck_storage_objects_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'attachment', 'document', 'system_asset'));
            ALTER TABLE storage_upload_sessions DROP CONSTRAINT IF EXISTS ck_storage_upload_sessions_purpose;
            ALTER TABLE storage_upload_sessions ADD CONSTRAINT ck_storage_upload_sessions_purpose CHECK (purpose IN ('legacy_image', 'profile_image', 'attachment', 'document', 'system_asset'));
            """);
    }

    private static void DeleteBlogImageStorageRows(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM storage_upload_sessions WHERE purpose = 'blog_image';
            DELETE FROM storage_objects WHERE purpose = 'blog_image';
            """);
    }
}
