@echo off

pushd %~dp0\..\..\src\QuinntyneBrown.Git.Cli

echo Packing QuinntyneBrown.Git.Cli...
dotnet pack -c Release

if %ERRORLEVEL% neq 0 (
    echo Pack failed.
    popd
    exit /b 1
)

echo Installing git-cli tool globally...
dotnet tool install --global --add-source .\bin\Release QuinntyneBrown.Git.Cli

if %ERRORLEVEL% neq 0 (
    echo Install failed. If already installed, try updating:
    echo   dotnet tool update --global --add-source .\bin\Release QuinntyneBrown.Git.Cli
    popd
    exit /b 1
)

echo git-cli installed successfully.
popd
