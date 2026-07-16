// ABOUTME: AutoMapper profile for shared category and custom-property entities.
// ABOUTME: Keeps generic taxonomy mappings independent from deleted product-specific templates.

using AutoMapper;
using Explore.Application.DTOs.Category;
using Explore.Application.DTOs.CustomPropertyDefinition;
using Explore.Domain;

namespace Explore.Application.Profiles;

public class CustomPropertyMappingProfile : Profile
{
    public CustomPropertyMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentFullName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.FullName : null));
        CreateMap<Category, CategoryListDto>()
            .ForMember(dest => dest.ParentFullName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.FullName : null));
        CreateMap<CreateCategoryDto, Category>();

        CreateMap<CustomPropertyOption, CustomPropertyOptionDto>();
        CreateMap<CustomPropertyDefinition, CustomPropertyDefinitionDto>();
        CreateMap<CustomPropertyDefinition, CustomPropertyDefinitionListDto>()
            .ForMember(dest => dest.OptionCount, opt => opt.MapFrom(src => src.Options.Count));
        CreateMap<CreateCustomPropertyDefinitionDto, CustomPropertyDefinition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultOptionId, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultOption, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ForMember(dest => dest.Values, opt => opt.Ignore());
        CreateMap<UpdateCustomPropertyDefinitionDto, CustomPropertyDefinition>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultOptionId, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultOption, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ForMember(dest => dest.Values, opt => opt.Ignore());
    }
}
