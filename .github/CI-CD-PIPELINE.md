# CI/CD Pipeline Documentation

This document describes the comprehensive CI/CD pipeline implemented for the Handlr CQRS framework.

## Overview

The pipeline consists of four main workflows:

1. **Continuous Integration (CI)** - Builds, tests, and validates code on every push/PR
2. **Release** - Automates versioning, packaging, and publishing releases
3. **Security** - Performs security scans and vulnerability checks
4. **Documentation** - Builds and deploys project documentation

## Workflows

### 1. Continuous Integration (`ci.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**Jobs:**
- **Test**: Runs tests on .NET 8.0 and 9.0, uploads code coverage
- **Build Samples**: Validates sample applications work correctly
- **Code Quality**: Checks code formatting and runs security scans
- **Package**: Creates NuGet packages for main branch builds

**Features:**
- Multi-target framework testing
- Code coverage reporting with Codecov
- NuGet package caching for faster builds
- Security vulnerability scanning
- Preview package publishing to GitHub Packages

### 2. Release (`release.yml`)

**Triggers:**
- Git tags matching `v*.*.*` pattern (e.g., `v1.0.0`)
- Manual workflow dispatch with version input

**Jobs:**
- **Validate Release**: Ensures version format and runs full test suite
- **Build and Pack**: Creates versioned NuGet packages
- **Create Release**: Generates GitHub release with auto-generated changelog
- **Publish NuGet**: Publishes packages to NuGet.org and GitHub Packages

**Features:**
- Automatic version detection from tags
- Semantic version validation
- Auto-generated release notes with commit history
- Dual publishing (NuGet.org + GitHub Packages)
- Production environment protection

### 3. Security (`security.yml`)

**Triggers:**
- Weekly schedule (Monday 2 AM UTC)
- Push to `main` branch
- Pull requests to `main` branch

**Jobs:**
- **Dependency Scan**: Checks for vulnerable and deprecated NuGet packages
- **CodeQL Analysis**: GitHub's semantic code analysis for security vulnerabilities
- **Secrets Scan**: Uses TruffleHog to detect leaked secrets

**Features:**
- Automated vulnerability detection
- Comprehensive security analysis
- Secret leak prevention
- Compliance reporting

### 4. Documentation (`docs.yml`)

**Triggers:**
- Push to `main` with changes to docs, markdown files, or source code
- Pull requests with documentation changes

**Jobs:**
- **Build Docs**: Generates API documentation with DocFX
- **Deploy Docs**: Publishes to GitHub Pages (main branch only)
- **Validate Links**: Checks all markdown links are working

**Features:**
- Automatic API documentation generation
- GitHub Pages deployment
- Link validation
- DocFX integration

## Required Secrets

To fully utilize the pipeline, configure these secrets in your GitHub repository:

### Repository Secrets

1. **`NUGET_API_KEY`** - API key for publishing to NuGet.org
   - Get from: https://www.nuget.org/account/apikeys
   - Scope: Push packages

2. **`CODECOV_TOKEN`** (Optional) - Token for code coverage reporting
   - Get from: https://codecov.io/
   - Improves coverage upload reliability

### GitHub Token

The `GITHUB_TOKEN` is automatically provided and used for:
- GitHub Packages publishing
- Creating releases
- Deploying to GitHub Pages

## Environment Setup

### Production Environment

Create a `production` environment in your repository settings with:
- **Protection rules**: Require manual approval for releases
- **Deployment branches**: Restrict to `main` branch only
- **Secrets**: Add `NUGET_API_KEY` here for secure access

### GitHub Pages

Enable GitHub Pages in repository settings:
- Source: GitHub Actions
- Branch: Automatically managed by workflow

## Usage Examples

### Creating a Release

1. **Automatic Release (Recommended)**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Manual Release**:
   - Go to Actions → Release workflow
   - Click "Run workflow"
   - Enter version (e.g., "1.0.0")

### Running Security Scans

Security scans run automatically, but you can trigger manually:
- Go to Actions → Security Scan workflow
- Click "Run workflow"

### Building Documentation

Documentation builds automatically on changes, or manually:
- Go to Actions → Documentation workflow
- Click "Run workflow"

## Pipeline Status Badges

Add these badges to your README:

```markdown
[![CI](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/ci.yml)
[![Release](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/release.yml/badge.svg)](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/release.yml)
[![Security](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/security.yml/badge.svg)](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/security.yml)
[![Docs](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/docs.yml/badge.svg)](https://github.com/YOUR_USERNAME/generated-cqrs/actions/workflows/docs.yml)
```

## Troubleshooting

### Common Issues

1. **NuGet API Key Issues**:
   - Ensure key has "Push" permissions
   - Check key hasn't expired
   - Verify package name doesn't conflict

2. **Documentation Build Failures**:
   - Check DocFX configuration
   - Ensure all referenced projects build successfully
   - Verify markdown syntax is correct

3. **Security Scan Failures**:
   - Update vulnerable packages identified
   - Review and fix CodeQL security issues
   - Remove any detected secrets

### Getting Help

- Check workflow logs in GitHub Actions tab
- Review this documentation
- Open an issue for pipeline-specific problems

## Maintenance

### Regular Tasks

1. **Monthly**: Review and update vulnerable dependencies
2. **Quarterly**: Update workflow actions to latest versions
3. **Annually**: Rotate API keys and secrets

### Updating Workflows

When modifying workflows:
1. Test in a feature branch first
2. Use workflow validation tools
3. Update this documentation accordingly

## Pipeline Metrics

Monitor these metrics for pipeline health:
- Build success rate
- Test coverage trends
- Security scan results
- Release cadence
- Documentation freshness

The pipeline is designed to be robust, secure, and maintainable while providing excellent developer experience.