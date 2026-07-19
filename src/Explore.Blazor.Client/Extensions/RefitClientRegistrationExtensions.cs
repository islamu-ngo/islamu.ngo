// ABOUTME: Registers shared Refit clients for Blazor BFF endpoints with secure handlers.
// ABOUTME: Requires host-supplied same-origin BaseAddress values so routes stay behind BFF/YARP.

using System.Text.Json;
using System.Text.Json.Serialization;
using Explore.Blazor.Client.Services.Http;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Explore.Blazor.Client.Extensions;

/// <summary>
/// Extensions for registering Refit clients securely.
/// </summary>
public static class RefitClientRegistrationExtensions
{
    /// <summary>
    /// Creates the default Refit settings matching the application's JSON serialization standards.
    /// Preserves camelCase, enum string mapping, and problem details.
    /// </summary>
    public static RefitSettings CreateDefaultRefitSettings()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Important: Use strings for enums
        jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };
    }

    /// <summary>
    /// Registers a secure Refit client for BFF (Backend-for-Frontend) endpoints.
    /// </summary>
    /// <typeparam name="TInterface">The Refit API interface</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configureClient">Optional configuration for the underlying HttpClient</param>
    public static IHttpClientBuilder AddBffRefitClient<TInterface>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient>? configureClient = null) where TInterface : class
    {
        var builder = services.AddRefitClient<TInterface>(CreateDefaultRefitSettings());

        builder.ConfigureHttpClient((sp, client) =>
        {
            configureClient?.Invoke(sp, client);

            if (client.BaseAddress is null)
            {
                throw new InvalidOperationException(
                    $"A BaseAddress is required for BFF Refit client {typeof(TInterface).Name}.");
            }
        })
        .AddHttpMessageHandler<BffAntiforgeryMessageHandler>()
        .AddHttpMessageHandler<BffUnauthorizedHandler>();

        return builder;
    }
}
