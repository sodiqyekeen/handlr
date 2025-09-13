; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
HANDLR001 | Usage | Error | Request class must implement ICommand or IQuery
HANDLR002 | Usage | Error | Handler class must implement ICommandHandler or IQueryHandler  
HANDLR003 | Usage | Warning | Handler implementation not found
HANDLR004 | Design | Error | Pipeline behavior must implement IPipelineBehavior
HANDLR005 | Design | Error | Duplicate handler found
HANDLR006 | Design | Error | Invalid return type for command
HANDLR007 | Design | Error | Invalid return type for query
HANDLR008 | Design | Error | Handler method signature is invalid
HANDLR009 | Design | Warning | Multiple constructors found
HANDLR010 | Usage | Info | Generated code formatting issue