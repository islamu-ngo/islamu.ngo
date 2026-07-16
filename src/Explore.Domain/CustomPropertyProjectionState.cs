// ABOUTME: Enum for custom-property projection rebuild worker states.
// ABOUTME: Generic state machine: Idle → Rebuilding → Idle or Failed.

namespace Explore.Domain;

public enum CustomPropertyProjectionState
{
    Idle,
    Rebuilding,
    Failed,
}
