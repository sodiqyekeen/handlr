# Installation Guide

Learn how to install and set up the Handlr CQRS Framework in your .NET 9.0 project.

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- Basic understanding of CQRS patterns

## Package Installation

### Using Package Manager Console

```powershell
Install-Package Handlr.Abstractions
Install-Package Handlr.SourceGenerator
```

### Using .NET CLI

```bash
dotnet add package Handlr.Abstractions
dotnet add package Handlr.SourceGenerator
```

### Using PackageReference

Add these to your `.csproj` file:

```xml
<PackageReference Include="Handlr.Abstractions" Version="1.0.0" />
<PackageReference Include="Handlr.SourceGenerator" Version="1.0.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

## Next Steps

- [Create your first command](first-command.md)
- [Set up dependency injection](di-setup.md)
- [Explore pipeline behaviors](../behaviors/overview.md)