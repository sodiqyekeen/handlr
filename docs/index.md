<div align="center">
  <img src="../assets/logos/handlr-logo.svg" alt="Handlr Logo" width="150" height="150" />
</div>

# Handlr CQRS Framework Documentation

Welcome to the comprehensive documentation for the Handlr CQRS Framework - a high-performance, developer-friendly CQRS implementation for .NET with switch expression-based dispatching.

## 🚀 Quick Start

Get started with the Handlr framework in minutes:

1. **Install the packages** - Core abstractions and source generator
2. **Define your commands and queries** - Use `BaseCommand<T>` and `BaseQuery<T>` 
3. **Implement handlers** - Normal classes implementing `ICommandHandler<T,R>` or `IQueryHandler<T,R>`
4. **Register and use** - Automatic discovery with high-performance dispatcher

## 📚 Documentation Sections

### Getting Started
- [Installation Guide](getting-started/installation.md)

### 🛠️ API Reference

Browse the complete API documentation:

- [Core Abstractions](../api/index.md)

## 🎯 Sample Projects

Explore working examples:

- **[Console Sample](https://github.com/sodiqyekeen/handlr/tree/main/samples/SampleConsoleApp)** - Complete console application with all behaviors
- **[Web API Sample](https://github.com/sodiqyekeen/handlr/tree/main/samples/SampleWebApi)** - ASP.NET Core Web API implementation

## 📖 Additional Resources

- [GitHub Repository](https://github.com/sodiqyekeen/handlr)
- [NuGet Packages](https://www.nuget.org/profiles/sodiqyekeen)
- [Release Notes](https://github.com/sodiqyekeen/handlr/releases)
- [Issues & Support](https://github.com/sodiqyekeen/handlr/issues)

## 🤝 Contributing

We welcome contributions! See our [Contributing Guide](https://github.com/sodiqyekeen/handlr/blob/main/CONTRIBUTING.md) for details.

---

*This documentation is automatically generated and deployed using DocFX and GitHub Actions.*