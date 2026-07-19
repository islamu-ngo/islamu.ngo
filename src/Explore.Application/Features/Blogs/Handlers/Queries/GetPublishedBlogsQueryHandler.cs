// ABOUTME: Handles published Blog list reads with tenant, publication, search, category, and tag filters.
// ABOUTME: Uses BlogQuerySpecification plus real CountAsync/ListAsync pagination before DTO mapping.

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

public sealed class GetPublishedBlogsQueryHandler(IBlogRepository blogRepository)
    : IRequestHandler<GetPublishedBlogsQuery, PaginatedResult<BlogSummaryDto>>
{
    public async Task<PaginatedResult<BlogSummaryDto>> Handle(
        GetPublishedBlogsQuery request,
        CancellationToken cancellationToken)
    {
        var validator = new GetPublishedBlogsQueryValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var (page, pageSize) = PaginatedResult<BlogSummaryDto>.NormalizeParameters(request.Page, request.PageSize);
        var specification = BlogQuerySpecification.Empty
            .ForTenant(request.TenantId)
            .PublishedAt(DateTime.UtcNow)
            .SortByPublishedAtDescending();

        if (request.CategoryId.HasValue)
        {
            specification = specification.WithCategory(request.CategoryId.Value);
        }

        if (request.TagId.HasValue)
        {
            specification = specification.WithTag(request.TagId.Value);
        }

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
