// ABOUTME: Supplies retained metadata for the Event product persistence teardown target model.
// ABOUTME: Reuses the static retained-model definition without constructing a DbContext.

using Explore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.Persistence.Migrations
{
    [DbContext(typeof(ExploreDbContext))]
    [Migration("20260716120000_RemoveEventProductSurface")]
    partial class RemoveEventProductSurface
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
            => ExploreDbContextModelSnapshot.BuildRetainedModel(modelBuilder);
    }
}
