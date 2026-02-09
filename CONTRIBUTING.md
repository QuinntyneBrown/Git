# Contributing to git-cli

Thanks for your interest in contributing! Here's how to get started.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/downloads) installed and available on PATH

## Getting Started

1. Fork and clone the repository
2. Open `Git.sln` in Visual Studio or your preferred editor
3. Build the solution:

```bash
dotnet build
```

## Project Structure

```
src/
  QuinntyneBrown.Git.Cli/
    Commands/       # CLI command definitions (clone, clean, purge)
    Services/       # Core services (git operations, temp folder management)
    Program.cs      # Entry point with DI/logging setup
eng/
  scripts/          # Build and install scripts
```

## Making Changes

1. Create a branch from `main` for your changes
2. Make your changes, following the existing code style
3. Ensure the project builds without errors:

```bash
dotnet build
```

4. Test locally by installing the tool:

```bash
eng\scripts\install-cli.bat
```

5. Commit your changes with a clear, descriptive message
6. Open a pull request against `main`

## Pull Requests

- Keep PRs focused on a single change
- Describe what the change does and why
- Reference any related issues

## Reporting Issues

Open an issue on the repository with:

- A clear description of the problem or suggestion
- Steps to reproduce (for bugs)
- Expected vs actual behavior
