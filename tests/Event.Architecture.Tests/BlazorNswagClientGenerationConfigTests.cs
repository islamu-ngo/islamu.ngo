// ABOUTME: Verifies Blazor client NSwag generation uses the canonical first-party ISLAMU contract.
// ABOUTME: Guards the build-time generator settings without requiring the generated client file to exist.

using System.Text.Json;
using System.Xml.Linq;

namespace Event.Architecture.Tests;

public sealed class BlazorNswagClientGenerationConfigTests
{
    [Test]
    public async Task BlazorClient_NswagGenerationConfig_MustUseCanonicalIslamuClientContract()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var nswagPath = Path.Combine(repositoryRoot, "src", "Explore.Blazor.Client", "nswag.json");
        var projectPath = Path.Combine(repositoryRoot, "src", "Explore.Blazor.Client", "Explore.Blazor.Client.csproj");

        var nswagText = await File.ReadAllTextAsync(nswagPath);
        var projectText = await File.ReadAllTextAsync(projectPath);
        using var nswagDocument = JsonDocument.Parse(nswagText);
        var root = nswagDocument.RootElement;
        var documentGenerator = root.GetProperty("documentGenerator").GetProperty("fromDocument");
        var csharpClient = root.GetProperty("codeGenerators").GetProperty("openApiToCSharpClient");

        await Assert.That(root.GetProperty("runtime").GetString()).IsEqualTo("Net100");
        await Assert.That(documentGenerator.GetProperty("json").GetString()).IsEqualTo("../../schemas/openapi_islamu.json");
        await Assert.That(csharpClient.GetProperty("className").GetString()).IsEqualTo("IslamuApiClient");
        await Assert.That(csharpClient.GetProperty("generateClientInterfaces").GetBoolean()).IsTrue();
        await Assert.That($"I{csharpClient.GetProperty("className").GetString()}").IsEqualTo("IIslamuApiClient");
        await Assert.That(csharpClient.GetProperty("namespace").GetString()).IsEqualTo("Explore.Blazor.Client.Clients");
        await Assert.That(csharpClient.GetProperty("clientClassAccessModifier").GetString()).IsEqualTo("public");
        await Assert.That(csharpClient.GetProperty("injectHttpClient").GetBoolean()).IsTrue();
        await Assert.That(csharpClient.GetProperty("disposeHttpClient").GetBoolean()).IsFalse();
        await Assert.That(csharpClient.GetProperty("httpClientType").GetString()).IsEqualTo("System.Net.Http.HttpClient");
        await Assert.That(csharpClient.GetProperty("useBaseUrl").GetBoolean()).IsFalse();
        await Assert.That(csharpClient.GetProperty("generateBaseUrlProperty").GetBoolean()).IsFalse();
        await Assert.That(csharpClient.GetProperty("jsonLibrary").GetString()).IsEqualTo("SystemTextJson");
        await Assert.That(csharpClient.GetProperty("generateNullableReferenceTypes").GetBoolean()).IsTrue();
        await Assert.That(csharpClient.GetProperty("dateType").GetString()).IsEqualTo("System.DateTimeOffset");
        await Assert.That(csharpClient.GetProperty("dateTimeType").GetString()).IsEqualTo("System.DateTimeOffset");
        await Assert.That(csharpClient.GetProperty("operationGenerationMode").GetString()).IsEqualTo("SingleClientFromOperationId");
        await Assert.That(csharpClient.GetProperty("output").GetString()).IsEqualTo("Clients/IslamuApiClient.g.cs");

        var project = XDocument.Parse(projectText);
        var properties = project.Descendants().Where(element => element.Parent?.Name.LocalName == "PropertyGroup").ToList();
        var target = project.Descendants().Single(element => element.Name.LocalName == "Target" && element.Attribute("Name")?.Value == "GenerateIslamuApiClient");
        var targetXml = target.ToString(SaveOptions.DisableFormatting);
        var exec = target.Descendants().Single(element => element.Name.LocalName == "Exec");
        var error = target.Descendants().Single(element => element.Name.LocalName == "Error");
        var compileInclude = target.Descendants().Single(element => element.Name.LocalName == "Compile" && element.Attribute("Include") is not null);

        await Assert.That(PropertyValue(properties, "NswagSchema")).IsEqualTo("$(MSBuildProjectDirectory)/../../schemas/openapi_islamu.json");
        await Assert.That(PropertyValue(properties, "NswagConfig")).IsEqualTo("$(MSBuildProjectDirectory)/nswag.json");
        await Assert.That(PropertyValue(properties, "NswagClientOutput")).IsEqualTo("$(MSBuildProjectDirectory)/Clients/IslamuApiClient.g.cs");
        await Assert.That(target.Attribute("BeforeTargets")?.Value).IsEqualTo("CoreCompile");
        await Assert.That(target.Attribute("Inputs")?.Value).IsEqualTo("$(NswagConfig);$(NswagSchema)");
        await Assert.That(target.Attribute("Outputs")?.Value).IsEqualTo("$(NswagClientOutput)");
        await Assert.That(error.Attribute("Condition")?.Value).IsEqualTo("!Exists('$(NswagSchema)')");
        await Assert.That(error.Attribute("Text")?.Value).Contains("Canonical OpenAPI schema not found");
        await Assert.That(exec.Attribute("Command")?.Value).Contains("dotnet tool run nswag run");
        await Assert.That(exec.Attribute("WorkingDirectory")?.Value).IsEqualTo("$(MSBuildProjectDirectory)");
        await Assert.That(compileInclude.Attribute("Include")?.Value).IsEqualTo("$(NswagClientOutput)");
        await Assert.That(compileInclude.Attribute("Condition")?.Value).IsEqualTo("Exists('$(NswagClientOutput)')");
        await Assert.That(targetXml).DoesNotContain(DeletedClientIdentity());
        await Assert.That(projectText).DoesNotContain(DeletedSchemaIdentity());
        await Assert.That(nswagText).DoesNotContain(DeletedClientIdentity());
        await Assert.That(nswagText).DoesNotContain(DeletedSchemaIdentity());
    }

    private static string PropertyValue(IEnumerable<XElement> properties, string name)
    {
        return properties
            .SelectMany(group => group.Elements())
            .Single(element => element.Name.LocalName == name)
            .Value;
    }

    private static string DeletedClientIdentity() => "Event" + "ApiClient";

    private static string DeletedSchemaIdentity() => "openapi_islamu" + "-event";

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
        {
            current = current.Parent;
        }

        return current?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the repository root from the architecture test output directory.");
    }
}
