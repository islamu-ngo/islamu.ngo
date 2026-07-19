// ABOUTME: Captures the retained persistence model after Blog foundation schema addition.
// ABOUTME: Delegates to immutable frozen Blog metadata for the current EF Core snapshot.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Explore.Persistence.Migrations
{
    [DbContext(typeof(ExploreDbContext))]
    partial class ExploreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
            => AddBlogFoundationTargetModel.Apply(modelBuilder);
    }
}
