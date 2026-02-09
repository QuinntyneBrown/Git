# git-cli

A .NET global tool and core library for cloning git repositories into managed temporary folders.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/downloads) installed and available on PATH

## Packages

| Package | Description |
|---------|-------------|
| **QuinntyneBrown.Git.Core** | Core library with git and temp folder services. Consumable by any .NET 8+ project. |
| **QuinntyneBrown.Git.Cli** | .NET global tool that exposes the core library as a CLI. |

## Installation

### CLI tool

```bash
# From the repo
eng\scripts\install-cli.bat

# Or manually
dotnet pack src\QuinntyneBrown.Git.Cli -c Release
dotnet tool install --global --add-source src\QuinntyneBrown.Git.Cli\bin\Release QuinntyneBrown.Git.Cli
```

### Core library

```bash
dotnet add package QuinntyneBrown.Git.Core
```

Register services via dependency injection:

```csharp
using QuinntyneBrown.Git.Core;

services.AddGitCore();
```

Then inject `IGitService` or `ITempFolderService` where needed.

## Uninstall

```bash
dotnet tool uninstall --global QuinntyneBrown.Git.Cli
```

## Commands

### clone

Clone a git repository to a temporary folder. Outputs the path to the temporary folder.

```bash
git-cli clone <url> [--branch <branch>] [--commit <commit>]
```

If the repository has already been cloned and matches the requested branch/commit, the existing path is returned. Otherwise the tool re-clones.

### clean

Delete the temporary folder for a specific git repository.

```bash
git-cli clean <url>
```

### purge

Delete all temporary folders created by the tool.

```bash
git-cli purge
```

## How It Works

Temporary folders are created under `%TEMP%/git-cli/` using a hash of the repository URL. Each cloned folder contains a `.git-cli-url` marker file for traceability. The tool drives the `git` command line directly, so git must be installed and on your PATH.

## Project Structure

```
src/
  QuinntyneBrown.Git.Core/
    Services/
      IGitService.cs              # Git operations contract
      GitService.cs               # Drives git CLI via Process
      ITempFolderService.cs       # Temp folder management contract
      TempFolderService.cs        # Temp folder lifecycle management
    ServiceCollectionExtensions.cs # DI registration (AddGitCore)
  QuinntyneBrown.Git.Cli/
    Commands/
      CloneCommand.cs             # git-cli clone <url>
      CleanCommand.cs             # git-cli clean <url>
      PurgeCommand.cs             # git-cli purge
    Program.cs                    # Entry point with DI/logging setup
eng/
  scripts/
    install-cli.bat               # Pack and install the tool globally
```

## Build

```bash
dotnet build
```
