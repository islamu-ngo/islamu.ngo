// ABOUTME: Supplies frozen metadata for the Blog foundation migration target model.
// ABOUTME: Delegates to immutable Blog target metadata rather than the mutable DbContext snapshot.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.Persistence.Migrations
{
    [DbContext(typeof(ExploreDbContext))]
    [Migration("20260716130000_AddBlogFoundation")]
    partial class AddBlogFoundation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
            => AddBlogFoundationTargetModel.Apply(modelBuilder);
    }
}
