// ABOUTME: Static migration tests for the Blog foundation schema operations and frozen metadata wiring.
// ABOUTME: Verifies migration shape without applying migrations or touching a database.

using System.Reflection;
using Explore.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Event.Persistence.IntegrationTests.Migrations;

public class AddBlogFoundationMigrationTests
{
    [Test]
    public async Task Up_CreatesBlogsBeforeJoinTables_AndDownDropsJoinTablesBeforeBlogs()
    {
        var upOperations = InvokeMigration("Up");
        var downOperations = InvokeMigration("Down");

        var createTables = upOperations.OfType<CreateTableOperation>().Select(operation => operation.Name).ToArray();
        var dropTables = downOperations.OfType<DropTableOperation>().Select(operation => operation.Name).ToArray();

        await Assert.That(createTables).IsEquivalentTo(["blogs", "blog_categories", "blog_tags"]);
        await Assert.That(Array.IndexOf(createTables, "blogs")).IsLessThan(Array.IndexOf(createTables, "blog_categories"));
        await Assert.That(Array.IndexOf(createTables, "blogs")).IsLessThan(Array.IndexOf(createTables, "blog_tags"));
        await Assert.That(dropTables).IsEquivalentTo(["blog_categories", "blog_tags", "blogs"]);
        await Assert.That(Array.IndexOf(dropTables, "blogs")).IsGreaterThan(Array.IndexOf(dropTables, "blog_categories"));
        await Assert.That(Array.IndexOf(dropTables, "blogs")).IsGreaterThan(Array.IndexOf(dropTables, "blog_tags"));
    }

    [Test]
    public async Task Up_AddsTenantSlugUniquenessAndTenantSafeCompositeForeignKeys()
    {
        var upOperations = InvokeMigration("Up");

        var uniqueIndexes = upOperations.OfType<CreateIndexOperation>()
            .Where(operation => operation.IsUnique)
            .ToDictionary(operation => operation.Name, operation => operation.Columns);
        var foreignKeys = upOperations.OfType<CreateTableOperation>()
            .SelectMany(operation => operation.ForeignKeys)
            .ToDictionary(operation => operation.Name, operation => operation);

        await Assert.That(uniqueIndexes["ux_blogs_tenant_slug"]).IsEquivalentTo(["tenant_id", "slug"]);
        await Assert.That(uniqueIndexes["ux_blog_categories_tenant_blog_category"]).IsEquivalentTo(["tenant_id", "blog_id", "category_id"]);
        await Assert.That(uniqueIndexes["ux_blog_tags_tenant_blog_tag"]).IsEquivalentTo(["tenant_id", "blog_id", "tag_id"]);
        await Assert.That(foreignKeys["fk_blogs_storage_objects_tenant_id_thumbnail_storage_object_id"].Columns).IsEquivalentTo(["tenant_id", "thumbnail_storage_object_id"]);
        await Assert.That(foreignKeys["fk_blog_categories_blogs_tenant_id_blog_id"].OnDelete).IsEqualTo(ReferentialAction.Cascade);
        await Assert.That(foreignKeys["fk_blog_categories_categories_tenant_id_category_id"].OnDelete).IsEqualTo(ReferentialAction.Restrict);
        await Assert.That(foreignKeys["fk_blog_tags_blogs_tenant_id_blog_id"].OnDelete).IsEqualTo(ReferentialAction.Cascade);
        await Assert.That(foreignKeys["fk_blog_tags_tags_tenant_id_tag_id"].OnDelete).IsEqualTo(ReferentialAction.Restrict);
    }

    [Test]
    public async Task UpAndDown_UpdateStoragePurposeConstraintsReversibly()
    {
        var upSql = InvokeMigration("Up").OfType<SqlOperation>().Single().Sql;
        var downSql = string.Join('\n', InvokeMigration("Down").OfType<SqlOperation>().Select(operation => operation.Sql));

        await Assert.That(upSql).Contains("ck_storage_objects_purpose");
        await Assert.That(upSql).Contains("ck_storage_upload_sessions_purpose");
        await Assert.That(upSql).Contains("blog_image");
        await Assert.That(downSql).Contains("ck_storage_objects_purpose");
        await Assert.That(downSql).Contains("ck_storage_upload_sessions_purpose");
        await Assert.That(downSql).Contains("DELETE FROM storage_upload_sessions WHERE purpose = 'blog_image';");
        await Assert.That(downSql).Contains("DELETE FROM storage_objects WHERE purpose = 'blog_image';");
        await Assert.That(downSql.IndexOf("DELETE FROM storage_upload_sessions", StringComparison.Ordinal))
            .IsLessThan(downSql.IndexOf("DELETE FROM storage_objects", StringComparison.Ordinal));
        await Assert.That(downSql.IndexOf("DELETE FROM storage_objects", StringComparison.Ordinal))
            .IsLessThan(downSql.IndexOf("ALTER TABLE storage_objects ADD CONSTRAINT", StringComparison.Ordinal));
    }

    [Test]
    public async Task FrozenMetadataWiring_UsesAddBlogFoundationHelperOnly()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var snapshotPath = Path.Combine(repositoryRoot, "src/Explore.Persistence/Migrations/ExploreDbContextModelSnapshot.cs");
        var designerPath = Path.Combine(repositoryRoot, "src/Explore.Persistence/Migrations/20260716130000_AddBlogFoundation.Designer.cs");
        var helperPath = Path.Combine(repositoryRoot, "src/Explore.Persistence/Migrations/AddBlogFoundationTargetModel.cs");

        var snapshot = await File.ReadAllTextAsync(snapshotPath);
        var designer = await File.ReadAllTextAsync(designerPath);
        var helper = await File.ReadAllTextAsync(helperPath);

        await Assert.That(snapshot).Contains("AddBlogFoundationTargetModel.Apply(modelBuilder)");
        await Assert.That(designer).Contains("AddBlogFoundationTargetModel.Apply(modelBuilder)");
        await Assert.That(designer).DoesNotContain("ExploreDbContextModelSnapshot");
        await Assert.That(helper).Contains("RemoveEventProductSurfaceTargetModel.Apply(modelBuilder)");
        await Assert.That(snapshot).DoesNotContain("RemoveEventProductSurfaceTargetModel.Apply(modelBuilder)");
    }

    [Test]
    public async Task FrozenTargetModels_RetainPostEventChassisEntitiesWithoutMissingNavigations()
    {
        var frozenModels = new[]
        {
            (Name: nameof(RemoveEventProductSurface), Model: new RemoveEventProductSurface().TargetModel),
            (Name: nameof(AddBlogFoundation), Model: new AddBlogFoundation().TargetModel)
        };
        var retainedEntities = new[]
        {
            (Name: "Explore.Domain.EmailDispatchOutbox", Table: "email_dispatch_outbox"),
            (Name: "Explore.Domain.IntegrationSyncOutbox", Table: "integration_sync_outbox"),
            (Name: "Explore.Domain.NotificationIntent", Table: "notification_intents"),
            (Name: "Explore.Domain.NotificationExternalDelegation", Table: "notification_external_delegations"),
            (Name: "Explore.Domain.OrganizationReview", Table: "organization_reviews")
        };

        foreach (var frozenModel in frozenModels)
        {
            foreach (var retainedEntity in retainedEntities)
            {
                var entityType = frozenModel.Model.FindEntityType(retainedEntity.Name);

                await Assert.That(entityType)
                    .IsNotNull()
                    .Because($"{frozenModel.Name} must retain {retainedEntity.Name} metadata");
                await Assert.That(entityType!.GetTableName()).IsEqualTo(retainedEntity.Table);
            }
        }
    }

    private static IReadOnlyList<MigrationOperation> InvokeMigration(string methodName)
    {
        var migration = new AddBlogFoundation();
        var builder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        var method = typeof(AddBlogFoundation).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

        method!.Invoke(migration, [builder]);

        return builder.Operations;
    }

    private static string ResolveRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src/Explore.Persistence")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test base directory.");
    }
}
