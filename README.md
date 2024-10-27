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

TODO: install, main APIs

## Tools

### RESX Converter

TODO: install, commands usage, example

### Localization linter

TODO: install, commands usage, example

## Build

The project requires .NET 8.0 SDK to build.

To build, test and generate artifacts run:

```sh
# Build and run tests (with code coverage!)
dotnet run --project build/orchestrator

# (Optional) Create bundles (nuget, zips, docs)
dotnet run --project build/orchestrator -- --target=Bundle
```

## Release

Create a new GitHub release with a tag `v{Version}` (e.g. `v2.4`) and that's it!
This triggers a pipeline that builds and deploy the project.
