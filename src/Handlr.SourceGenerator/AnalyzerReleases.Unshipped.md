; Unshipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.1

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
HANDLR001 | Usage | Error | Removed: Simplified source generator - no longer validates request implementation
HANDLR002 | Usage | Error | Removed: Simplified source generator - no longer validates handler implementation  
HANDLR003 | Usage | Warning | Removed: Simplified source generator - handlers resolved at runtime
HANDLR004 | Design | Error | Removed: Simplified source generator - no longer validates pipeline behaviors
HANDLR005 | Design | Error | Removed: Simplified source generator - duplicate handlers handled at runtime
HANDLR006 | Design | Error | Removed: Simplified source generator - return types validated at compile time
HANDLR007 | Design | Error | Removed: Simplified source generator - return types validated at compile time
HANDLR008 | Design | Error | Removed: Simplified source generator - method signatures validated at compile time
HANDLR009 | Design | Warning | Removed: Simplified source generator - constructor validation removed
HANDLR010 | Usage | Info | Removed: Simplified source generator - formatting handled by IDE