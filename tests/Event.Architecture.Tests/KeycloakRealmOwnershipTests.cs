// ABOUTME: Locks the public Keycloak exports to first-party Event and NGO realm resources.
// ABOUTME: Rejects private Control Plane identifiers with bounded, secret-free diagnostics.

using System.Text.Json;

namespace Event.Architecture.Tests;

public sealed class KeycloakRealmOwnershipTests
{
    private static readonly string RepoRoot = ResolveRepoRoot();
    private static readonly string[] ExpectedClientIds =
    [
        "islamu-event-blazor",
        "islamu-event-api",
        "islamu-ngo-blazor",
        "islamu-ngo-api"
    ];

    private static readonly string[] ReservedIdentifiers =
    [
        "islamu-event-control-plane",
        "event-control-plane-api",
        "event-control-plane-partner-automation",
        "control-plane-operator"
    ];

    private static readonly string[] ProvisionerForbiddenIdentifiers =
    [
        "control-plane",
        "partner_account_id",
        "partner-automation",
        "partner automation",
        "control-plane-operator",
        "ensure_realm_role",
        "ensure_client_scope",
        "ensure_partner",
        "detach_all_client_scopes",
        "scope-mapping"
    ];

    [Test]
    [Arguments("docker/keycloak/realm-export.json")]
    [Arguments("docker/keycloak/ISLAMU-realm.test.json")]
    public async Task RealmExportMustContainOnlyFirstPartyOwnedClients(string relativePath)
    {
        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(RepoRoot, relativePath)));
        var root = document.RootElement;

        await Assert.That(root.GetProperty("realm").GetString()).IsEqualTo("ISLAMU");

        var clientIds = root.GetProperty("clients")
            .EnumerateArray()
            .Select(client => client.GetProperty("clientId").GetString() ?? string.Empty)
            .ToArray();
        await Assert.That(clientIds.Length).IsEqualTo(ExpectedClientIds.Length);
        await Assert.That(clientIds).IsEquivalentTo(ExpectedClientIds)
            .Because($"{relativePath} must contain exactly the Event clients plus additive NGO clients.");

        await AssertAudienceMapperAsync(
            root,
            ownerClientId: "islamu-event-blazor",
            mapperName: "islamu-event-api-audience",
            audienceClientId: "islamu-event-api");
        await AssertAudienceMapperAsync(
            root,
            ownerClientId: "islamu-ngo-blazor",
            mapperName: "islamu-ngo-api-audience",
            audienceClientId: "islamu-ngo-api");
        await Assert.That(ClientHasMapper(root, "islamu-event-api", "islamu-event-api-audience")).IsFalse();
        await Assert.That(ClientHasMapper(root, "islamu-ngo-api", "islamu-ngo-api-audience")).IsFalse();

        var ngoBlazorClient = GetClient(root, "islamu-ngo-blazor");
        await Assert.That(ngoBlazorClient.GetProperty("publicClient").GetBoolean()).IsFalse();
        await Assert.That(ngoBlazorClient.GetProperty("standardFlowEnabled").GetBoolean()).IsTrue();
        await Assert.That(ngoBlazorClient.GetProperty("directAccessGrantsEnabled").GetBoolean()).IsFalse();
        await Assert.That(ngoBlazorClient.GetProperty("serviceAccountsEnabled").GetBoolean()).IsFalse();
        await Assert.That(ngoBlazorClient.GetProperty("attributes").GetProperty("pkce.code.challenge.method").GetString()).IsEqualTo("S256");

        var ngoApiClient = GetClient(root, "islamu-ngo-api");
        await Assert.That(ngoApiClient.GetProperty("bearerOnly").GetBoolean()).IsTrue();
        await Assert.That(ngoApiClient.GetProperty("standardFlowEnabled").GetBoolean()).IsFalse();
        await Assert.That(ngoApiClient.GetProperty("directAccessGrantsEnabled").GetBoolean()).IsFalse();
        await Assert.That(ngoApiClient.GetProperty("serviceAccountsEnabled").GetBoolean()).IsFalse();

        var violations = CollectReservedIdentifierViolations(root);
        await Assert.That(violations).IsEmpty()
            .Because(string.Join('\n', violations));
    }

    [Test]
    public async Task TestRealmFixtureMustKeepExpectedUsers()
    {
        using var document = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(RepoRoot, "docker/keycloak/ISLAMU-realm.test.json")));

        var users = document.RootElement.GetProperty("users")
            .EnumerateArray()
            .Select(user => user.GetProperty("username").GetString() ?? string.Empty)
            .ToArray();

        await Assert.That(users).IsEquivalentTo(
            ["test-admin", "test-user", "test-tenant-admin", "test-wrong-issuer"]);
    }

    [Test]
    public async Task ProvisionerSourceMustStayNgoOwnedOnly()
    {
        var source = await File.ReadAllTextAsync(
            Path.Combine(RepoRoot, "docker/keycloak/provision-islamu-ngo-client.sh"));

        await Assert.That(source).Contains("KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED");
        await Assert.That(source).Contains("KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID");
        await Assert.That(source).Contains("KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID");
        await Assert.That(source).Contains("KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME");
        await Assert.That(source).Contains("set -euo pipefail");
        await Assert.That(source).Contains("umask 077");
        await Assert.That(source).Contains("mktemp");
        await Assert.That(source).Contains("mktemp -d");
        await Assert.That(source).Contains("--config \"$KCADM_CONFIG\"");
        await Assert.That(source).Contains("KEYCLOAK_ADMIN_REALM");
        await Assert.That(source).Contains("KEYCLOAK_ALLOW_INSECURE_HTTP");
        await Assert.That(source).Contains("KC_CLI_CLIENT_SECRET");
        await Assert.That(source).Contains("KC_CLI_PASSWORD");
        await Assert.That(source).Contains("validate_boolean \"KEYCLOAK_ISLAMU_NGO_PROVISIONING_REQUIRED\"");
        await Assert.That(source).Contains("validate_boolean \"KEYCLOAK_ALLOW_INSECURE_HTTP\"");
        await Assert.That(source).Contains("validate_secret \"KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_SECRET\"");
        await Assert.That(source).Contains("validate_json_string_array");
        await Assert.That(source).Contains("created_by_concurrent_run");
        await Assert.That(source).Contains("--fields \"id,$field\"");
        await Assert.That(source).Contains("exact_resource_uuid clients \"clientId=$1\" clientId");
        await Assert.That(source).Contains("exact_resource_uuid \"clients/$uuid/protocol-mappers/models\" \"name=$mapper_name\" name");
        await Assert.That(source).Contains("Duplicate Keycloak resources");
        await Assert.That(source).Contains("validate_owned_resource_names");
        await Assert.That(source).Contains("OWNED_NGO_BLAZOR_CLIENT_ID=\"islamu-ngo-blazor\"");
        await Assert.That(source).Contains("OWNED_NGO_API_CLIENT_ID=\"islamu-ngo-api\"");
        await Assert.That(source).Contains("OWNED_NGO_AUDIENCE_MAPPER_NAME=\"islamu-ngo-api-audience\"");
        await Assert.That(source).Contains("require_exact_owned_identifier \"KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID\" \"$KEYCLOAK_ISLAMU_NGO_BLAZOR_CLIENT_ID\" \"$OWNED_NGO_BLAZOR_CLIENT_ID\"");
        await Assert.That(source).Contains("require_exact_owned_identifier \"KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID\" \"$KEYCLOAK_ISLAMU_NGO_API_CLIENT_ID\" \"$OWNED_NGO_API_CLIENT_ID\"");
        await Assert.That(source).Contains("require_exact_owned_identifier \"KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME\" \"$KEYCLOAK_ISLAMU_NGO_AUDIENCE_MAPPER_NAME\" \"$OWNED_NGO_AUDIENCE_MAPPER_NAME\"");
        await Assert.That(source).Contains("Refusing override/collision value");
        await Assert.That(source).Contains("islamu-event-blazor|islamu-event-api|islamu-event-api-audience");
        await Assert.That(source).Contains("secret redacted");
        await Assert.That(source).Contains("KEYCLOAK_ADMIN_SERVICE_CLIENT_ID");
        await Assert.That(ExtractShellFunction(source, "exact_resource_uuid")).DoesNotContain("|| true");

        await Assert.That(source).DoesNotContain("--password");
        await Assert.That(source).DoesNotContain("--client-secret");
        await Assert.That(source).DoesNotContain("create realms");
        await Assert.That(source).DoesNotContain("delete realms");
        await Assert.That(source).DoesNotContain("update \"realms/$KEYCLOAK_REALM\"");

        foreach (var forbiddenIdentifier in ProvisionerForbiddenIdentifiers)
        {
            await Assert.That(source).DoesNotContain(forbiddenIdentifier);
        }
    }

    [Test]
    public async Task ProvisionerDockerfileMustPinKeycloakAndCopyOnlyNgoScript()
    {
        var source = await File.ReadAllTextAsync(
            Path.Combine(RepoRoot, "docker/keycloak/Dockerfile.provisioner"));

        await Assert.That(source).Contains("FROM quay.io/keycloak/keycloak:26.5.2");
        await Assert.That(source).Contains("COPY --chmod=0555 provision-islamu-ngo-client.sh");
        await Assert.That(source).Contains("/opt/islamu-ngo/bin/provision-islamu-ngo-client.sh");

        foreach (var forbiddenIdentifier in ProvisionerForbiddenIdentifiers)
        {
            await Assert.That(source).DoesNotContain(forbiddenIdentifier);
        }
    }

    [Test]
    public async Task ReservedIdentifierCollectorReportsBoundedDiagnosticAndCleansTemporaryFixture()
    {
        var violations = await CollectViolationsFromTemporaryFixtureAsync(
            """
            {
              "realm": "ISLAMU",
              "clients": [{ "clientId": "event-control-plane-api" }],
              "clientScopes": [],
              "roles": { "realm": [] },
              "users": []
            }
            """);

        await Assert.That(violations).IsEquivalentTo(
            ["client ID contains reserved identifier 'event-control-plane-api'."]);
        await Assert.That(violations.All(violation => violation.Length <= 128)).IsTrue();
    }

    [Test]
    [Arguments(
        """
        {
          "clients": [
            { "clientId": "islamu-event-blazor", "description": "event-control-plane-api" },
            { "clientId": "islamu-event-api" }
          ],
          "clientScopes": [],
          "roles": { "realm": [] },
          "users": []
        }
        """,
        "client contains reserved identifier 'event-control-plane-api'.")]
    [Arguments(
        """
        {
          "clients": [
            { "clientId": "islamu-event-blazor" },
            { "clientId": "islamu-event-api" }
          ],
          "clientScopes": [],
          "roles": { "realm": { "name": "control-plane-operator" } },
          "users": []
        }
        """,
        "roles.realm must be an array.")]
    [Arguments(
        """
        {
          "clients": [
            { "clientId": "islamu-event-blazor" },
            { "clientId": "islamu-event-api" }
          ],
          "clientScopes": [],
          "defaultDefaultClientScopes": ["event-control-plane-partner-automation"],
          "roles": { "realm": [] },
          "users": []
        }
        """,
        "client scope contains reserved identifier 'event-control-plane-partner-automation'.")]
    public async Task ReservedIdentifierCollectorRejectsFailOpenBypassFixtures(
        string fixtureJson,
        string expectedViolation)
    {
        var violations = await CollectViolationsFromTemporaryFixtureAsync(fixtureJson);

        await Assert.That(violations).Contains(expectedViolation);
        await Assert.That(violations.All(violation => violation.Length <= 128)).IsTrue();
    }

    private static string[] CollectReservedIdentifierViolations(JsonElement root)
    {
        var violations = new HashSet<string>(StringComparer.Ordinal);
        if (!IsExpectedObject(root, "root", violations))
        {
            return violations.ToArray();
        }

        foreach (var client in EnumerateArray(root, "clients", "clients", violations))
        {
            if (!IsExpectedObject(client, "clients[]", violations))
            {
                continue;
            }

            CollectScopeReferences(
                client,
                "defaultClientScopes",
                "clients[].defaultClientScopes",
                violations);
            CollectScopeReferences(
                client,
                "optionalClientScopes",
                "clients[].optionalClientScopes",
                violations);

            foreach (var property in client.EnumerateObject())
            {
                if (property.Name is "protocolMappers" or "defaultClientScopes" or "optionalClientScopes")
                {
                    continue;
                }

                var category = property.NameEquals("clientId") ? "client ID" : "client";
                CollectStringViolations(property.Value, category, violations);
            }

            CollectMapperViolations(client, "clients[].protocolMappers", violations);
        }

        CollectScopeReferences(
            root,
            "defaultDefaultClientScopes",
            "defaultDefaultClientScopes",
            violations);
        CollectScopeReferences(
            root,
            "defaultOptionalClientScopes",
            "defaultOptionalClientScopes",
            violations);

        foreach (var clientScope in EnumerateArray(root, "clientScopes", "clientScopes", violations))
        {
            if (!IsExpectedObject(clientScope, "clientScopes[]", violations))
            {
                continue;
            }

            foreach (var property in clientScope.EnumerateObject())
            {
                if (!property.NameEquals("protocolMappers"))
                {
                    CollectStringViolations(property.Value, "client scope", violations);
                }
            }

            CollectMapperViolations(clientScope, "clientScopes[].protocolMappers", violations);
        }

        if (root.TryGetProperty("roles", out var roles))
        {
            if (!IsExpectedObject(roles, "roles", violations))
            {
                return violations.Order(StringComparer.Ordinal).ToArray();
            }

            foreach (var realmRole in EnumerateArray(roles, "realm", "roles.realm", violations))
            {
                CollectStringViolations(realmRole, "realm role", violations);
            }
        }

        foreach (var user in EnumerateArray(root, "users", "users", violations))
        {
            CollectStringViolations(user, "user", violations);
        }

        return violations.Order(StringComparer.Ordinal).ToArray();
    }

    private static async Task AssertAudienceMapperAsync(
        JsonElement root,
        string ownerClientId,
        string mapperName,
        string audienceClientId)
    {
        var mapper = GetClient(root, ownerClientId)
            .GetProperty("protocolMappers")
            .EnumerateArray()
            .Single(mapper => mapper.GetProperty("name").GetString() == mapperName);
        var config = mapper.GetProperty("config");

        await Assert.That(mapper.GetProperty("protocolMapper").GetString()).IsEqualTo("oidc-audience-mapper");
        await Assert.That(config.GetProperty("included.client.audience").GetString()).IsEqualTo(audienceClientId);
        await Assert.That(config.GetProperty("id.token.claim").GetString()).IsEqualTo("false");
        await Assert.That(config.GetProperty("access.token.claim").GetString()).IsEqualTo("true");
    }

    private static bool ClientHasMapper(JsonElement root, string clientId, string mapperName)
    {
        var client = GetClient(root, clientId);
        return client.TryGetProperty("protocolMappers", out var mappers)
            && mappers.EnumerateArray().Any(mapper => mapper.GetProperty("name").GetString() == mapperName);
    }

    private static JsonElement GetClient(JsonElement root, string clientId) =>
        root.GetProperty("clients")
            .EnumerateArray()
            .Single(client => client.GetProperty("clientId").GetString() == clientId);

    private static string ExtractShellFunction(string source, string functionName)
    {
        var marker = $"{functionName}() {{";
        var start = source.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Could not find shell function '{functionName}'.");
        }

        var end = source.IndexOf("\n}\n", start, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new InvalidOperationException($"Could not find end of shell function '{functionName}'.");
        }

        return source[start..(end + 3)];
    }

    private static async Task<string[]> CollectViolationsFromTemporaryFixtureAsync(string fixtureJson)
    {
        var fixturePath = Path.Combine(
            Path.GetTempPath(),
            $"event-keycloak-realm-ownership-{Guid.NewGuid():N}.json");
        string[] violations;

        try
        {
            await File.WriteAllTextAsync(fixturePath, fixtureJson);
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(fixturePath));
            violations = CollectReservedIdentifierViolations(document.RootElement);
        }
        finally
        {
            File.Delete(fixturePath);
        }

        await Assert.That(File.Exists(fixturePath)).IsFalse();
        return violations;
    }

    private static void CollectScopeReferences(
        JsonElement owner,
        string propertyName,
        string path,
        HashSet<string> violations)
    {
        foreach (var clientScope in EnumerateArray(owner, propertyName, path, violations))
        {
            CollectStringViolations(clientScope, "client scope", violations);
        }
    }

    private static void CollectMapperViolations(
        JsonElement owner,
        string path,
        HashSet<string> violations)
    {
        foreach (var mapper in EnumerateArray(owner, "protocolMappers", path, violations))
        {
            if (!IsExpectedObject(mapper, $"{path}[]", violations))
            {
                continue;
            }

            if (mapper.TryGetProperty("name", out var name))
            {
                CollectStringViolations(name, "mapper name", violations);
            }

            if (mapper.TryGetProperty("config", out var config))
            {
                CollectStringViolations(config, "mapper config", violations);
            }
        }
    }

    private static void CollectStringViolations(
        JsonElement element,
        string category,
        HashSet<string> violations)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var value = element.GetString();
                foreach (var reservedIdentifier in ReservedIdentifiers)
                {
                    if (value?.Contains(reservedIdentifier, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        violations.Add($"{category} contains reserved identifier '{reservedIdentifier}'.");
                    }
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectStringViolations(item, category, violations);
                }

                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    CollectStringViolations(property.Value, category, violations);
                }

                break;
        }
    }

    private static IEnumerable<JsonElement> EnumerateArray(
        JsonElement owner,
        string propertyName,
        string path,
        HashSet<string> violations)
    {
        if (!owner.TryGetProperty(propertyName, out var array))
        {
            yield break;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            violations.Add($"{path} must be an array.");
            yield break;
        }

        foreach (var item in array.EnumerateArray())
        {
            yield return item;
        }
    }

    private static bool IsExpectedObject(
        JsonElement element,
        string path,
        HashSet<string> violations)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        violations.Add($"{path} must be an object.");
        return false;
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Explore.slnx"))
                && Directory.Exists(Path.Combine(current.FullName, "docker", "keycloak")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repository root containing Explore.slnx and docker/keycloak.");
    }
}
