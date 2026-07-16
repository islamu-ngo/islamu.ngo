// ABOUTME: Hard-limit quota setting definitions for Layer 3 custom properties (Rule 16).
// ABOUTME: Each quota has a tenant-overridable default and a platform maximum encoded in the description for governance review.

namespace Explore.Domain.Settings.Definitions;

public static class CustomPropertyQuotaSettingDefinitions
{
    private const string Category = "CustomPropertyQuotas";

    public static readonly SettingDefinition MaxDefinitionsPerTenantPerEntityScope = new(
        Key: "custom_properties.max_definitions_per_tenant_per_entity_scope",
        ValueType: SettingValueType.Integer,
        DefaultValue: "500",
        Category: Category,
        Description: "Maximum custom-property definitions per tenant per entity scope (Organization/Group). Platform max: 5000.",
        MaxScope: SettingScope.Tenant);

    public static readonly SettingDefinition MaxOptionsPerDefinition = new(
        Key: "custom_properties.max_options_per_definition",
        ValueType: SettingValueType.Integer,
        DefaultValue: "200",
        Category: Category,
        Description: "Maximum option rows allowed per custom-property definition. Platform max: 2000.",
        MaxScope: SettingScope.Tenant);

    public static readonly SettingDefinition MaxMultiValueRowsPerValue = new(
        Key: "custom_properties.max_multi_value_rows_per_value",
        ValueType: SettingValueType.Integer,
        DefaultValue: "20",
        Category: Category,
        Description: "Maximum per-ordinal value rows allowed for a multi-valued custom property. Platform max: 200.",
        MaxScope: SettingScope.Tenant);

    public static IReadOnlyList<SettingDefinition> All =>
    [
        MaxDefinitionsPerTenantPerEntityScope,
        MaxOptionsPerDefinition,
        MaxMultiValueRowsPerValue,
    ];
}
