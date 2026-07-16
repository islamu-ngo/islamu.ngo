// ABOUTME: Source-generated System.Text.Json metadata resolver for the Application layer.
// ABOUTME: Leaves unknown payload types to the configured reflection resolver without product-specific contracts.

using System.Text.Json.Serialization;

namespace Explore.Application.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    Converters = [typeof(JsonStringEnumConverter)])]
[JsonSerializable(typeof(object))]
public partial class ExploreJsonContext : JsonSerializerContext;
