# IL28n

<!-- markdownlint-disable MD033 -->
<p align="center">
  <a href="https://github.com/pleonex/IL28n/actions/workflows/build-and-release.yml">
    <img alt="Build and release" src="https://github.com/pleonex/IL28n/actions/workflows/build-and-release.yml/badge.svg" />
  </a>
  &nbsp;
  <a href="https://choosealicense.com/licenses/mit/">
    <img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat" />
  </a>
  &nbsp;
</p>

Internalization (i18n) and localization (l10n) framework and tools for software
made with .NET (languages based on IL).

> [!WARNING]  
> This is a personal project with **no support**. The project may not have an
> active development. Don't expect new features, fixes (including security
> fixes). I don't recommend using it for production environments. Feel free to
> fork and adapt. _Small_ contributions are welcome.

## Libraries

This project provides the following libraries as NuGet packages (via nuget.org).
The libraries support the latest version of .NET and its LTS.

- [![PleOps.Il28n.Formats.Resx](https://img.shields.io/nuget/v/PleOps.Il28n.Formats.Resx?label=PleOps.Il28n.Formats.Resx&logo=nuget)](https://www.nuget.org/packages/PleOps.Il28n.Formats.Resx):
  read .NET RESX files and converters into PO format.

**Preview releases** can be found in this
[Azure DevOps NuGet repository](https://dev.azure.com/pleonex/Pleosoft/_artifacts/feed/Pleosoft-Preview).
To use a preview release, create a file `nuget.config` in the same directory of
your solution file (.sln) with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear/>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="PleOps-Preview" value="https://pkgs.dev.azure.com/pleonex/Pleosoft/_packaging/Pleosoft-Preview/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="PleOps-Preview">
      <package pattern="PleOps.Il28n.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Then restore / install as usual via Visual Studio, Rider or command-line. You
may need to restart Visual Studio for the changes to apply.

## Tools

### RESX Converter

> Command-line application to convert RESX files into PO format.

#### Installation

The program is distributed as a portable command-line from the release page, and
as a _dotnet tool_.

To install as a _dotnet tool_ follow:

1. Install the
   [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Install the latest version of the tool:
   `dotnet tool install -g PleOps.Il28n.ResxConverter`
   - You can update it with `dotnet tool update -g PleOps.Il28n.ResxConverter`
   - To use preview versions, add the arguments
     `--prerelease --add-source https://pkgs.dev.azure.com/pleonex/Pleosoft/_packaging/Pleosoft-Preview/nuget/v3/index.json`

#### Commands

- `resx2po`: convert a pair of RESX files (default and language specific) into
  PO format.
- `resx2pot`: convert a RESX file (usually default culture) into a PO template
  format.

Run with the `--help` argument to see the help page with each argument.

#### Examples

- Create a PO file from `MyResource.resx` and its Spanish translation
  (`MyResource.es.resx`):

```sh
# Example for Unix shells. For PowerShell replace \ with `, for batch use ^
resxconverter resx2po \
  --file "MyResource.resx" \
  --resx-lang "es" \
  --output "Output/es" \
  --project-id "Example project" \
  --reporter "@pleonex" \
  --po-lang "es-ES"
```

- Create PO files for all the RESX with the default culture (`<name>.rex`) and
  their translation in Spanish (`<name>.es.resx`):

```sh
resxconverter resx2po \
  --directory "Resources" --recursive \
  --resx-lang "es" \
  --output "Output/es" \
  --project-id "Example project" \
  --reporter "@pleonex" \
  --po-lang "es-ES"
```

### Localization linter

TODO: install, commands usage, example

## Build

The project requires .NET 8.0 SDK to build.

To build, test and generate artifacts run:

```sh
# Build and run tests
dotnet run --project build/orchestrator

# (Optional) Create bundles (nuget, zips, docs)
dotnet run --project build/orchestrator -- --target=Bundle
```

## Release

Create a new GitHub release with a tag `v{Version}` (e.g. `v2.4`) and that's it!
This triggers a pipeline that builds and deploy the project.
