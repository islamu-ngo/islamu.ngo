// ABOUTME: Enum identifying the scope type for a custom-property projection dirty scope.
// ABOUTME: Retained as part of the generic projection infrastructure; values are opaque to the schema.

namespace Explore.Domain;

public enum CustomPropertyProjectionScopeType
{
    Event = 0,
    EventSession = 1,
}
