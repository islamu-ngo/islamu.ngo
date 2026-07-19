// ABOUTME: FluentValidation contracts for Blog request DTOs and MediatR request identifiers.
// ABOUTME: Enforces bounded public pagination and required tenant, blog, Markdown, category, and tag IDs.

using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Requests.Commands;
using FluentValidation;

namespace Explore.Application.Features.Blogs.Validators;

public sealed class GetPublishedBlogsQueryValidator : AbstractValidator<GetPublishedBlogsQuery>
{
    public GetPublishedBlogsQueryValidator()
    {
        RuleFor(query => query.TenantId).NotEmpty();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.CategoryId).MustBeNullOrNonEmpty();
        RuleFor(query => query.TagId).MustBeNullOrNonEmpty();
    }
}

public sealed class GetPublishedBlogBySlugQueryValidator : AbstractValidator<GetPublishedBlogBySlugQuery>
{
    public GetPublishedBlogBySlugQueryValidator()
    {
        RuleFor(query => query.TenantId).NotEmpty();
        RuleFor(query => query.Slug).NotEmpty().MaximumLength(200);
    }
}

public sealed class GetBlogsForManagementQueryValidator : AbstractValidator<GetBlogsForManagementQuery>
{
    public GetBlogsForManagementQueryValidator()
    {
        RuleFor(query => query.TenantId).NotEmpty();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetBlogByIdForManagementQueryValidator : AbstractValidator<GetBlogByIdForManagementQuery>
{
    public GetBlogByIdForManagementQueryValidator()
    {
        RuleFor(query => query.TenantId).NotEmpty();
        RuleFor(query => query.BlogId).NotEmpty();
    }
}

public sealed class CreateBlogCommandValidator : AbstractValidator<CreateBlogCommand>
{
    public CreateBlogCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.Blog).NotNull().SetValidator(new CreateBlogDtoValidator());
    }
}

public sealed class UpdateBlogCommandValidator : AbstractValidator<UpdateBlogCommand>
{
    public UpdateBlogCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.BlogId).NotEmpty();
        RuleFor(command => command.Blog).NotNull().SetValidator(new UpdateBlogDtoValidator());
    }
}

public sealed class PublishBlogCommandValidator : AbstractValidator<PublishBlogCommand>
{
    public PublishBlogCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.BlogId).NotEmpty();
    }
}

public sealed class UnpublishBlogCommandValidator : AbstractValidator<UnpublishBlogCommand>
{
    public UnpublishBlogCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.BlogId).NotEmpty();
    }
}

public sealed class DeleteBlogCommandValidator : AbstractValidator<DeleteBlogCommand>
{
    public DeleteBlogCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.BlogId).NotEmpty();
    }
}

public sealed class CreateBlogDtoValidator : AbstractValidator<CreateBlogDto>
{
    public CreateBlogDtoValidator()
    {
        this.RuleForBlogPayload(dto => dto);
    }
}

public sealed class UpdateBlogDtoValidator : AbstractValidator<UpdateBlogDto>
{
    public UpdateBlogDtoValidator()
    {
        this.RuleForBlogPayload(dto => dto);
    }
}

internal static class BlogValidatorRules
{
    public static IRuleBuilderOptions<T, Guid?> MustBeNullOrNonEmpty<T>(this IRuleBuilder<T, Guid?> rule)
    {
        return rule.Must(id => id is null || id.Value != Guid.Empty)
            .WithMessage("Identifier must be non-empty when provided.");
    }

    public static void RuleForBlogPayload<T>(this AbstractValidator<T> validator, Func<T, CreateBlogDto> payload)
    {
        validator.RuleFor(instance => payload(instance).Title)
            .NotEmpty()
            .MaximumLength(300);

        validator.RuleFor(instance => payload(instance).Slug)
            .MaximumLength(200);

        validator.RuleFor(instance => payload(instance).ContentMarkdown)
            .NotEmpty();

        validator.RuleFor(instance => payload(instance).ThumbnailStorageObjectId)
            .MustBeNullOrNonEmpty();

        validator.RuleForEach(instance => payload(instance).CategoryIds)
            .NotEmpty();

        validator.RuleForEach(instance => payload(instance).TagIds)
            .NotEmpty();
    }

    public static void RuleForBlogPayload<T>(this AbstractValidator<T> validator, Func<T, UpdateBlogDto> payload)
    {
        validator.RuleFor(instance => payload(instance).Title)
            .NotEmpty()
            .MaximumLength(300);

        validator.RuleFor(instance => payload(instance).Slug)
            .MaximumLength(200);

        validator.RuleFor(instance => payload(instance).ContentMarkdown)
            .NotEmpty();

        validator.RuleFor(instance => payload(instance).ExpectedConcurrencyStamp)
            .NotEmpty();

        validator.RuleFor(instance => payload(instance).ThumbnailStorageObjectId)
            .MustBeNullOrNonEmpty();

        validator.RuleForEach(instance => payload(instance).CategoryIds)
            .NotEmpty();

        validator.RuleForEach(instance => payload(instance).TagIds)
            .NotEmpty();
    }
}
