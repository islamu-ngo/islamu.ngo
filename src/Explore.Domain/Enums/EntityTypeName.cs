// ABOUTME: Enum discriminator identifying which entity type a custom property definition applies to.
// ABOUTME: Used by the EAV system to scope property definitions to Organization or Group.

namespace Explore.Domain.Enums;

public enum EntityTypeName
{
    Organization = 2,
    Group = 3
}
