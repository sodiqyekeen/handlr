using System;
using Handlr.Generated;

namespace SampleConsoleApp;

public static class TestDebug
{
    public static void PrintDebugInfo()
    {
        Console.WriteLine($"Source Generator Debug Info:");
        Console.WriteLine($"Generated at: {DebugInfo.GeneratedAt}");
        Console.WriteLine($"Commands found: {DebugInfo.CommandsFound}");
        Console.WriteLine($"Queries found: {DebugInfo.QueriesFound}");
        Console.WriteLine($"Behaviors found: {DebugInfo.BehaviorsFound}");
        Console.WriteLine($"Custom handlers found: {DebugInfo.CustomHandlersFound}");
        Console.WriteLine($"Element groups: {DebugInfo.ElementGroups}");
        Console.WriteLine($"Found classes: {string.Join(", ", DebugInfo.FoundClasses)}");
    }
}