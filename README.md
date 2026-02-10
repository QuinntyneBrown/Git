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
git-cli clone <url> [--branch <branch>] [--commit <commit>] [--tag <tag>]
git-cli clone --tag <tag>
```

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--branch` | `-b` | | The branch to clone or verify |
| `--commit` | | | The commit to clone or verify |
| `--tag` | `-t` | | A label to save or look up an existing clone |

If the repository has already been cloned and matches the requested branch/commit, the existing path is returned. Otherwise the tool re-clones.

Tags let you bookmark a clone so you can retrieve it later without the full URL:

```bash
# Clone and tag it
git-cli clone https://github.com/user/repo --tag my-repo

# Later, retrieve by tag alone
git-cli clone --tag my-repo
```

If a tag does not exist, the tool prints a message with usage guidance:

```
Tag 'my-repo' does not exist. Provide a URL to clone the repository:
  git-cli clone <url> --tag my-repo
```

### diff

Show the diff between a branch inferred from the URL and the default branch.

```bash
git-cli diff <url> [--default-branch <branch>] [--output <path>] [--include-files <path>]
```

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--default-branch` | `-d` | `main` | The base branch to diff against |
| `--output` | `-o` | *(stdout)* | File path to write the diff to |
| `--include-files` | | | Write an additional file with the default branch contents optimized for LLM consumption |

The branch is inferred from the URL (e.g. `https://github.com/user/repo/tree/feature-branch`). The repo is cloned to a temporary folder if not already cached. When `--include-files` is provided, the full contents of the default branch are written to the specified path using the same token-efficient XML format as the `files` command.

### files

Aggregate all repository text files into a single file optimized for LLM consumption.

```bash
git-cli files <url> [--branch <branch>] [--output <path>]
```

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--branch` | `-b` | *(default branch)* | The branch to clone or verify |
| `--output` | `-o` | `<repo-name>.txt` in CWD | File path to write the output to |

Files are emitted in a token-efficient XML format:

```xml
<file path="src/Program.cs">
file contents
</file>
```

Binary files, build output (`bin/`, `obj/`), and common non-text directories (`.git`, `node_modules`, `dist`, etc.) are automatically excluded.

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
    GitUrlParser.cs               # Extracts repo URL and branch from full URLs
    RepoContentAggregator.cs      # Aggregates repo files for LLM consumption
    ServiceCollectionExtensions.cs # DI registration (AddGitCore)
  QuinntyneBrown.Git.Cli/
    Commands/
      CloneCommand.cs             # git-cli clone
      CleanCommand.cs             # git-cli clean
      PurgeCommand.cs             # git-cli purge
      DiffCommand.cs              # git-cli diff
      FilesCommand.cs             # git-cli files
    Program.cs                    # Entry point with DI/logging setup
eng/
  scripts/
    install-cli.bat               # Pack and install the tool globally
```

## Build

```bash
dotnet build
```
