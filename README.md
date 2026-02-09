# git-cli

A .NET global tool for cloning git repositories into managed temporary folders.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/downloads) installed and available on PATH

## Installation

```bash
# From the repo
eng\scripts\install-cli.bat

# Or manually
dotnet pack src\QuinntyneBrown.Git.Cli -c Release
dotnet tool install --global --add-source src\QuinntyneBrown.Git.Cli\bin\Release QuinntyneBrown.Git.Cli
```

## Uninstall

```bash
dotnet tool uninstall --global QuinntyneBrown.Git.Cli
```

## Commands

### clone

Clone a git repository to a temporary folder. Outputs the path to the temporary folder.

```bash
git-cli clone <url>
```

If the repository has already been cloned, the existing path is returned.

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
  QuinntyneBrown.Git.Cli/
    Commands/
      CloneCommand.cs      # git-cli clone <url>
      CleanCommand.cs      # git-cli clean <url>
      PurgeCommand.cs      # git-cli purge
    Services/
      IGitService.cs        # Git operations contract
      GitService.cs         # Drives git CLI via Process
      ITempFolderService.cs # Temp folder management contract
      TempFolderService.cs  # Temp folder lifecycle management
    Program.cs              # Entry point with DI/logging setup
eng/
  scripts/
    install-cli.bat         # Pack and install the tool globally
```

## Build

```bash
dotnet build
```
