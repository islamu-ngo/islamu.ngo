// ABOUTME: Supplies retained metadata for the Event product persistence teardown target model.
// ABOUTME: Delegates to immutable frozen metadata rather than the mutable DbContext snapshot.

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
            => RemoveEventProductSurfaceTargetModel.Apply(modelBuilder);
    }
}
