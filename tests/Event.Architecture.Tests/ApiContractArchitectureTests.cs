// ABOUTME: Architecture tests enforcing API contract stability invariants at compile time.
// ABOUTME: Every [Http*] action must have Name= and response metadata; ApiExplorer-hidden endpoints are exempted.

namespace Event.Architecture.Tests;

using System.Linq;
using System.Reflection;
using Explore.API.Hateoas;
using Explore.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NetArchTest.Rules;

public class ApiContractArchitectureTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Explore.Application.ApplicationServicesRegistration).Assembly;

    private static readonly Type[] HttpVerbAttributes =
    {
        typeof(HttpGetAttribute),
        typeof(HttpPostAttribute),
        typeof(HttpPutAttribute),
        typeof(HttpPatchAttribute),
        typeof(HttpDeleteAttribute),
        typeof(HttpOptionsAttribute),
        typeof(HttpHeadAttribute)
    };

    [Test]
    [DisplayName("ApiProblemCodes must expose every initial catalog code")]
    public async Task ApiProblemCodes_MustExpose_EveryInitialCatalogCode()
    {
        var apiProblemCodesType = ApiAssembly.GetType("Explore.API.ExceptionHandling.ApiProblemCodes")
            ?? throw new InvalidOperationException("ApiProblemCodes type not found.");
        var codes = apiProblemCodesType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);

        var expectedCodes = new[]
        {
            "validation_failed",
            "tenant_required",
            "authentication_required",
            "forbidden",
            "resource_not_found",
            "resource_conflict",
            "concurrency_conflict",
            "duplicate_request",
            "rate_limited",
            "unexpected_error"
        };

        await Assert.That(expectedCodes.Except(codes, StringComparer.Ordinal)).IsEmpty()
            .Because("the Phase 2 API error catalog requires stable machine-readable codes for every initial ProblemDetails category.");
    }

    [Test]
    [DisplayName("Central API exception and validation writers must emit ProblemDetails code extensions")]
    public async Task CentralProblemDetailsWriters_MustEmit_CodeExtensions()
    {
        var sourceRoot = LocateSourceRoot();
        var sourceFiles = new[]
        {
            Path.Combine(sourceRoot, "src", "Explore.API", "ExceptionHandling", "GlobalExceptionHandler.cs"),
            Path.Combine(sourceRoot, "src", "Explore.API", "ExceptionHandling", "ValidationExceptionHandler.cs"),
            Path.Combine(sourceRoot, "src", "Explore.API", "ExceptionHandling", "ApiValidationProblemDetailsFactory.cs")
        };

        var violations = new List<string>();
        foreach (var sourceFile in sourceFiles)
        {
            var source = await File.ReadAllTextAsync(sourceFile);
            if (!source.Contains("Extensions[\"code\"]", StringComparison.Ordinal))
            {
                violations.Add(Path.GetRelativePath(sourceRoot, sourceFile));
            }
        }

        await Assert.That(violations).IsEmpty()
            .Because("every central ProblemDetails writer must attach the stable `code` extension required by dev/active/backend-api-health-refactor/api-error-catalog.md.");
    }

    [Test]
    [DisplayName("DTO enum properties must be registered in the OpenAPI string-enum schema catalog")]
    public async Task DtoEnumProperties_MustBeRegisteredIn_OpenApiStringEnumSchemaCatalog()
    {
        var dtoEnumTypes = ApplicationAssembly
            .GetExportedTypes()
            .Where(IsPublicDtoContractType)
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .SelectMany(property => CollectEnumTypes(property.PropertyType))
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();

        var catalogType = ApiAssembly.GetType("Explore.API.OpenApi.OpenApiStringEnumSchemaCatalog")
            ?? throw new InvalidOperationException("OpenApiStringEnumSchemaCatalog type not found.");
        var enumTypesProperty = catalogType.GetProperty("EnumTypes", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("OpenApiStringEnumSchemaCatalog.EnumTypes property not found.");
        var catalogEnumTypes = ((IEnumerable<Type>?)enumTypesProperty.GetValue(null) ?? Enumerable.Empty<Type>())
            .ToHashSet();

        var missing = dtoEnumTypes
            .Where(enumType => !catalogEnumTypes.Contains(enumType))
            .Select(enumType => enumType.FullName ?? enumType.Name)
            .ToList();

        await Assert.That(missing).IsEmpty()
            .Because("every enum exposed by public API DTOs must be registered in OpenApiStringEnumSchemaCatalog so OpenAPI emits string enum schemas matching the API JSON string-enum contract.");
    }


    [Test]
    [DisplayName("Controllers must not return raw missing-user Unauthorized strings")]
    public async Task Controllers_MustNotReturn_RawMissingUserUnauthorizedStrings()
    {
        var sourceRoot = LocateSourceRoot();
        var controllersRoot = Path.Combine(sourceRoot, "src", "Explore.API", "Controllers");
        var violations = Directory
            .EnumerateFiles(controllersRoot, "*.cs", SearchOption.TopDirectoryOnly)
            .Select(file => new
            {
                File = file,
                Source = File.ReadAllText(file)
            })
            .Where(candidate => candidate.Source.Contains("Unauthorized(\"User ID not found in token\")", StringComparison.Ordinal))
            .Select(candidate => Path.GetRelativePath(sourceRoot, candidate.File))
            .ToList();

        await Assert.That(violations).IsEmpty()
            .Because("missing authenticated user identifiers must use catalog-backed ProblemDetails with the `authentication_required` code instead of raw string 401 responses.");
    }

    [Test]
    [DisplayName("Every non-hidden [Http*] action must have Name= set")]
    public async Task EveryNonHiddenAction_MustHave_RouteName()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .And()
            .AreNotAbstract()
            .GetTypes()
            .ToList();

        var violations = new List<string>();

        foreach (var controller in controllerTypes)
        {
            var controllerHidden = controller
                .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true)
                .Cast<ApiExplorerSettingsAttribute>()
                .Any(a => a.IgnoreApi);

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsHttpAction)
                .ToList();

            foreach (var action in actions)
            {
                if (controllerHidden)
                {
                    continue;
                }

                var actionHidden = action
                    .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true)
                    .Cast<ApiExplorerSettingsAttribute>()
                    .Any(a => a.IgnoreApi);

                if (actionHidden)
                {
                    continue;
                }

                var httpAttr = action.GetCustomAttributes(true)
                    .FirstOrDefault(a => HttpVerbAttributes.Any(h => h.IsInstanceOfType(a)));

                if (httpAttr is not IRouteTemplateProvider { Name: not null and not "" })
                {
                    violations.Add($"{controller.Name}.{action.Name}");
                }
            }
        }

        await Assert.That(violations).IsEmpty()
            .Because("every non-hidden HTTP action must set Name= on its [Http*] attribute for stable operationIds; see docs/GOVERNANCE.md#api-contract-rules");
    }

    [Test]
    [Skip("Category: API contract. Removal: enable after Phase 0B fills missing [ProducesResponseType] metadata for backend-contract-risk-register.md R-005 and dev/active/backend-api-health-refactor/backend-api-health-refactor-tasks.md Phase 0B.")]
    [DisplayName("Every non-hidden [Http*] action must declare response metadata")]
    public async Task EveryNonHiddenAction_MustDeclare_ResponseMetadata()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .And()
            .AreNotAbstract()
            .GetTypes()
            .ToList();

        var violations = new List<string>();

        foreach (var controller in controllerTypes)
        {
            if (IsHiddenFromApiExplorer(controller))
            {
                continue;
            }

            var controllerResponseMetadata = controller
                .GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true)
                .ToList();

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsHttpAction)
                .ToList();

            foreach (var action in actions)
            {
                if (IsHiddenFromApiExplorer(action))
                {
                    continue;
                }

                var hasResponseMetadata = controllerResponseMetadata.Count > 0
                    || action.GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true).Any();

                if (!hasResponseMetadata)
                {
                    violations.Add($"{controller.Name}.{action.Name}");
                }
            }
        }

        await Assert.That(violations).IsEmpty()
            .Because("every public API operation must declare explicit response metadata before Phase 2 can enforce ProblemDetails-specific error contracts; see dev/active/backend-api-health-refactor/backend-api-health-refactor-tasks.md Phase 0B.");
    }

    [Test]
    [DisplayName("Actions with response metadata must declare a 2xx success response")]
    public async Task ActionsWithResponseMetadata_MustDeclare_SuccessResponse()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .And()
            .AreNotAbstract()
            .GetTypes()
            .ToList();

        var violations = new List<string>();

        foreach (var controller in controllerTypes)
        {
            if (IsHiddenFromApiExplorer(controller))
            {
                continue;
            }

            var controllerResponseMetadata = controller
                .GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true)
                .ToList();

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsHttpAction)
                .ToList();

            foreach (var action in actions)
            {
                if (IsHiddenFromApiExplorer(action))
                {
                    continue;
                }

                var responseMetadata = controllerResponseMetadata
                    .Concat(action.GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true))
                    .ToList();

                if (responseMetadata.Count == 0)
                {
                    continue;
                }

                if (!responseMetadata.Any(attribute => IsSuccessStatusCode(attribute.StatusCode)))
                {
                    violations.Add($"{controller.Name}.{action.Name}");
                }
            }
        }

        await Assert.That(violations).IsEmpty()
            .Because("actions that opt into response metadata must include at least one explicit 2xx success response so OpenAPI consumers can distinguish success from ProblemDetails/error responses.");
    }

    [Test]
    [DisplayName("Route names must be unique across all controllers")]
    public async Task RouteNames_MustBe_Unique()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .And()
            .AreNotAbstract()
            .GetTypes()
            .ToList();

        var names = new Dictionary<string, string>();

        foreach (var controller in controllerTypes)
        {
            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsHttpAction)
                .ToList();

            foreach (var action in actions)
            {
                var httpAttr = action.GetCustomAttributes(true)
                    .FirstOrDefault(a => HttpVerbAttributes.Any(h => h.IsInstanceOfType(a)));

                if (httpAttr is IRouteTemplateProvider { Name: string name })
                {
                    if (names.TryGetValue(name, out var existing))
                    {
                        names[name] = $"{existing}; {controller.Name}.{action.Name}";
                    }
                    else
                    {
                        names[name] = $"{controller.Name}.{action.Name}";
                    }
                }
            }
        }

        var duplicates = names
            .Where(kvp => kvp.Value.Contains(';'))
            .Select(kvp => $"{kvp.Key}: used by {kvp.Value}")
            .ToList();

        await Assert.That(duplicates).IsEmpty()
            .Because("route names must be unique; duplicates cause operationId collisions in OpenAPI");
    }

    [Test]
    [DisplayName("Every controller action must have a unique operation identity (controller.action)")]
    public async Task EveryAction_MustHave_UniqueIdentity()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .And()
            .AreNotAbstract()
            .GetTypes()
            .ToList();

        var identities = new HashSet<string>();
        var duplicates = new List<string>();

        foreach (var controller in controllerTypes)
        {
            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsHttpAction)
                .ToList();

            foreach (var action in actions)
            {
                var identity = $"{controller.Name}.{action.Name}";
                if (!identities.Add(identity))
                {
                    duplicates.Add(identity);
                }
            }
        }

        await Assert.That(duplicates).IsEmpty()
            .Because("every action must be uniquely identifiable by controller.action; overloaded actions break OpenAPI generation");
    }

    private static bool IsHttpAction(MethodInfo method)
    {
        if (method.IsSpecialName)
        {
            return false;
        }

        foreach (var attr in HttpVerbAttributes)
        {
            if (method.GetCustomAttributes(attr, inherit: true).Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Type> CollectEnumTypes(Type type)
    {
        var underlyingNullableType = Nullable.GetUnderlyingType(type);
        if (underlyingNullableType is not null)
        {
            type = underlyingNullableType;
        }

        if (type.IsEnum)
        {
            yield return type;
            yield break;
        }

        if (type.IsArray && type.GetElementType() is { } elementType)
        {
            foreach (var enumType in CollectEnumTypes(elementType))
            {
                yield return enumType;
            }

            yield break;
        }

        if (!type.IsGenericType || type == typeof(string))
        {
            yield break;
        }

        foreach (var genericArgument in type.GetGenericArguments())
        {
            foreach (var enumType in CollectEnumTypes(genericArgument))
            {
                yield return enumType;
            }
        }
    }

    private static bool IsPublicDtoContractType(Type type)
        => type.Namespace?.StartsWith("Explore.Application.DTOs", StringComparison.Ordinal) == true
           && !type.Namespace.Contains(".Validators", StringComparison.Ordinal)
           && !type.Name.EndsWith("Validator", StringComparison.Ordinal);

    private static string LocateSourceRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Explore.slnx")) &&
                (Directory.Exists(Path.Combine(current.FullName, "Explore.API")) ||
                 Directory.Exists(Path.Combine(current.FullName, "src", "Explore.API"))))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing Explore.slnx and Explore.API.");
    }

    private static bool IsHiddenFromApiExplorer(MemberInfo member)
    {
        return member.GetCustomAttributes<ApiExplorerSettingsAttribute>(inherit: true)
            .Any(attribute => attribute.IgnoreApi);
    }

    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices;
    }
}
