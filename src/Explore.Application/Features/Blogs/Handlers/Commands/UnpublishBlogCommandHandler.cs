// ABOUTME: Handles Blog unpublication by loading the tenant-owned aggregate and clearing publication state.
// ABOUTME: Persists the aggregate through IBlogRepository while preserving entity-first boundaries.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Domain;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Commands;

public sealed class UnpublishBlogCommandHandler(IBlogRepository blogRepository)
    : IRequestHandler<UnpublishBlogCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        UnpublishBlogCommand request,
        CancellationToken cancellationToken)
    {
        var validator = new UnpublishBlogCommandValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var blog = await blogRepository.GetByTenantAndIdAsync(request.TenantId, request.BlogId, cancellationToken);
        if (blog is null)
        {
            throw new NotFoundException(nameof(Blog), request.BlogId);
        }

        blog.Unpublish();
        var updated = await blogRepository.UpdateAsync(blog, cancellationToken);

        return new BaseCommandResponse<Guid>
        {
            Id = updated.Id,
            Success = true,
            Message = "Blog unpublished successfully."
        };
    }
}
