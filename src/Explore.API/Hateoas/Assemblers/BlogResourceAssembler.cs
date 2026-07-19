// ABOUTME: Resource assembler for Blog HAL detail and collection resources.
// ABOUTME: Connects Blog DTOs to the central HATEOAS link-generation pipeline.

namespace Explore.API.Hateoas.Assemblers;

using Explore.Application.Contracts.Hateoas;
using Explore.Application.DTOs.Blog;

public sealed class BlogResourceAssembler : ResourceAssemblerBase<BlogDetailDto, BlogSummaryDto>
{
    public BlogResourceAssembler(
        IHateoasLinkGenerator linkGenerator,
        ILinkPolicy<BlogDetailDto> detailLinkPolicy,
        ICollectionLinkPolicy<BlogSummaryDto> collectionLinkPolicy)
        : base(linkGenerator, detailLinkPolicy, collectionLinkPolicy)
    {
    }
}
