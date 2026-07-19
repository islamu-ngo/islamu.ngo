// ABOUTME: Handles authenticated Blog management list reads across draft and published posts.
// ABOUTME: Reuses BlogQuerySpecification for tenant filtering, optional title search, paging, and DTO mapping.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Application.Specifications;
using FluentValidation;
using MediatR;
using DomainBlog = Explore.Domain.Blog;

namespace Explore.Application.Features.Blogs.Handlers.Queries;

public sealed class GetBlogsForManagementQueryHandler(IBlogRepository blogRepository)
    : IRequestHandler<GetBlogsForManagementQuery, PaginatedResult<BlogSummaryDto>>
{
    public async Task<PaginatedResult<BlogSummaryDto>> Handle(
        GetBlogsForManagementQuery request,
        CancellationToken cancellationToken)
    {
        var validator = new GetBlogsForManagementQueryValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var (page, pageSize) = PaginatedResult<BlogSummaryDto>.NormalizeParameters(request.Page, request.PageSize);
        var specification = BlogQuerySpecification.Empty
            .ForTenant(request.TenantId)
            .SortByPublishedAtDescending();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            specification = specification.SearchTitle(request.Search);
        }

        int totalCount = await blogRepository.CountAsync(specification, cancellationToken);
        IReadOnlyList<DomainBlog> blogs = await blogRepository.ListAsync(
            specification,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return PaginatedResult<BlogSummaryDto>.Create(
            blogs.Select(BlogDtoMapper.ToSummary).ToList(),
            totalCount,
            page,
            pageSize);
    }
}
