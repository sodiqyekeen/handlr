using System.Threading.Tasks;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Represents a void return type for commands without results.
/// Used in pipeline behaviors to maintain type consistency.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Gets the singleton instance of Unit.
    /// </summary>
    public static Unit Value { get; } = new();

    /// <summary>
    /// Returns a task that represents a completed Unit value.
    /// </summary>
    public static Task<Unit> Task { get; } = System.Threading.Tasks.Task.FromResult(Value);
}
