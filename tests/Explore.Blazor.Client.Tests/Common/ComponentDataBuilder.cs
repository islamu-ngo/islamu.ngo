using Bogus;

namespace Explore.Blazor.Client.Tests.Common;

/// <summary>
/// Bogus-based fake data generators for Blazor client DTOs.
/// Follows the DataBuilder pattern established in Event.Application.UnitTests.
/// </summary>
/// <remarks>
/// <para>
/// Key type mappings:
/// - Price: double? (not decimal)
/// - CreateOrganizationDto.Postcode: int (not string)
/// - OrganizationListDto.Postcode: string
/// </para>
/// </remarks>
public static class ComponentDataBuilder
{
    #region Organization DTOs

    /// <summary>
    /// Generates fake OrganizationListDto for list displays.
    /// Property names match IslamuApiClient.g.cs (line 25785+)
    /// Postcode is string in OrganizationListDto (verified)
    /// </summary>
    public static Faker<OrganizationListDto> OrganizationListDto => new Faker<OrganizationListDto>()
        .RuleFor(o => o.Id, f => Guid.NewGuid())
        .RuleFor(o => o.FullName, f => f.Company.CompanyName())
        .RuleFor(o => o.Email, f => f.Internet.Email())
        .RuleFor(o => o.Country, f => f.Address.Country())
        .RuleFor(o => o.City, f => f.Address.City())
        .RuleFor(o => o.Address, f => f.Address.StreetAddress())
        // Postcode is string in OrganizationListDto (verified from API line 25806)
        .RuleFor(o => o.Postcode, f => f.Address.ZipCode())
        .RuleFor(o => o.WebsiteUrl, f => f.Internet.Url())
        .RuleFor(o => o.ApprovalStatusId, f => f.Random.Int(1, 4))
        .RuleFor(o => o.ApprovalStatusFullName, f => f.PickRandom("Pending", "Approved", "Rejected", "Suspended"));

    /// <summary>
    /// Generates fake OrganizationDto for detail views.
    /// Property names match IslamuApiClient.g.cs (line 25694+)
    /// Postcode is string in OrganizationDto (verified)
    /// </summary>
    public static Faker<OrganizationDto> OrganizationDto => new Faker<OrganizationDto>()
        .RuleFor(o => o.Id, f => Guid.NewGuid())
        .RuleFor(o => o.FullName, f => f.Company.CompanyName())
        .RuleFor(o => o.Email, f => f.Internet.Email())
        .RuleFor(o => o.Country, f => f.Address.Country())
        .RuleFor(o => o.City, f => f.Address.City())
        .RuleFor(o => o.Address, f => f.Address.StreetAddress())
        // Postcode is string in OrganizationDto (verified from API line 25715)
        .RuleFor(o => o.Postcode, f => f.Address.ZipCode())
        .RuleFor(o => o.WebsiteUrl, f => f.Internet.Url())
        .RuleFor(o => o.ApprovalStatusId, f => f.Random.Int(1, 4))
        .RuleFor(o => o.ApprovalStatusFullName, f => f.PickRandom("Pending", "Approved", "Rejected", "Suspended"));

    /// <summary>
    /// Generates fake CreateOrganizationDto for form testing.
    /// Property names match IslamuApiClient.g.cs (line 24045+)
    /// IMPORTANT: Postcode is int in CreateOrganizationDto (verified from API line 24063)
    /// </summary>
    public static Faker<CreateOrganizationDto> CreateOrganizationDto => new Faker<CreateOrganizationDto>()
        .RuleFor(o => o.FullName, f => f.Company.CompanyName())
        .RuleFor(o => o.Email, f => f.Internet.Email())
        .RuleFor(o => o.Country, f => f.Address.Country())
        .RuleFor(o => o.City, f => f.Address.City())
        .RuleFor(o => o.Address, f => f.Address.StreetAddress())
        // Postcode is int in CreateOrganizationDto (verified from API line 24063)
        .RuleFor(o => o.Postcode, f => f.Random.Int(10000, 99999))
        .RuleFor(o => o.WebsiteUrl, f => f.Internet.Url());

    #endregion

    #region Lookup Table DTOs

    /// <summary>
    /// Generates fake CategoryListDto for category dropdowns.
    /// </summary>
    public static Faker<CategoryListDto> CategoryListDto => new Faker<CategoryListDto>()
        .RuleFor(c => c.Id, f => Guid.NewGuid())
        .RuleFor(c => c.FullName, f => f.Commerce.Categories(1)[0])
        .RuleFor(c => c.MasterCode, f => f.Random.AlphaNumeric(10).ToUpper());

    /// <summary>
    /// Generates fake TagListDto for tag selections.
    /// </summary>
    public static Faker<TagListDto> TagListDto => new Faker<TagListDto>()
        .RuleFor(t => t.Id, f => Guid.NewGuid())
        .RuleFor(t => t.FullName, f => f.Lorem.Word())
        .RuleFor(t => t.MasterCode, f => f.Random.AlphaNumeric(10).ToUpper());

    /// <summary>
    /// Generates fake LocationListDto for location dropdowns.
    /// Property names match IslamuApiClient.g.cs (line 25610+)
    /// NOTE: LocationListDto does NOT have Postcode, Latitude, or Longitude properties
    /// </summary>
    public static Faker<LocationListDto> LocationListDto => new Faker<LocationListDto>()
        .RuleFor(l => l.Id, f => Guid.NewGuid())
        .RuleFor(l => l.FullName, f => f.Company.CompanyName())
        .RuleFor(l => l.Address, f => f.Address.StreetAddress())
        .RuleFor(l => l.City, f => f.Address.City())
        .RuleFor(l => l.Country, f => f.Address.Country())
        .RuleFor(l => l.Timezone, f => f.PickRandom("UTC", "Europe/London", "America/New_York"));

    /// <summary>
    /// Generates fake LanguageListDto for language dropdown.
    /// </summary>
    public static Faker<LanguageListDto> LanguageListDto => new Faker<LanguageListDto>()
        .RuleFor(l => l.Id, f => f.IndexFaker + 1)
        .RuleFor(l => l.FullName, f => f.PickRandom("English", "Arabic", "French", "German", "Turkish"))
        .RuleFor(l => l.MasterCode, f => f.PickRandom("EN", "AR", "FR", "DE", "TR"));

    /// <summary>
    /// Generates fake MadhabListDto for madhab dropdown.
    /// </summary>
    public static Faker<MadhabListDto> MadhabListDto => new Faker<MadhabListDto>()
        .RuleFor(m => m.Id, f => f.IndexFaker + 1)
        .RuleFor(m => m.FullName, f => f.PickRandom("Hanafi", "Maliki", "Shafi'i", "Hanbali"))
        .RuleFor(m => m.MasterCode, f => f.Random.AlphaNumeric(5).ToUpper());

    #endregion

    #region User & Session DTOs

    /// <summary>
    /// Generates fake UserDto for user displays.
    /// </summary>
    public static Faker<UserDto> UserDto => new Faker<UserDto>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.EmailVerified, f => f.Random.Bool());

    #endregion

    #region Response DTOs

    /// <summary>
    /// Generates a successful BaseCommandResponseOfGuid.
    /// </summary>
    /// <param name="id">Optional specific ID to return</param>
    public static BaseCommandResponseOfGuid SuccessResponse(Guid? id = null) => new()
    {
        Success = true,
        Id = id ?? Guid.NewGuid(),
        Message = "Operation completed successfully."
    };

    /// <summary>
    /// Generates a failed BaseCommandResponseOfGuid with errors.
    /// </summary>
    /// <param name="errors">Error messages to include</param>
    public static BaseCommandResponseOfGuid FailureResponse(params string[] errors) => new()
    {
        Success = false,
        Message = "Operation failed.",
        Errors = errors.ToList()
    };

    /// <summary>
    /// Generates a validation failure response.
    /// </summary>
    /// <param name="fieldName">Field that failed validation</param>
    /// <param name="message">Validation error message</param>
    public static BaseCommandResponseOfGuid ValidationFailureResponse(string fieldName, string message) => new()
    {
        Success = false,
        Message = "Validation failed.",
        Errors = new List<string> { $"{fieldName}: {message}" }
    };

    #endregion
}
