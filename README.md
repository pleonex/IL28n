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

#### RESX Converter: installation

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

#### RESX Converter: commands

- `resx2po`: convert a pair of RESX files (default and language specific) into
  PO format.
- `resx2pot`: convert a RESX file (usually default culture) into a PO template
  format.

Run with the `--help` argument to see the help page with each argument.

#### RESX Converter: examples

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

> Command-line application to run localization quality assurance checks on files

#### Localization linter: installation

The program is distributed as a portable command-line from the release page, and
as a _dotnet tool_.

To install as a _dotnet tool_ follow:

1. Install the
   [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Install the latest version of the tool:
   `dotnet tool install -g PleOps.Il28n.LocalizationLinter`
   - You can update it with
     `dotnet tool update -g PleOps.Il28n.LocalizationLinter`
   - To use preview versions, add the arguments
     `--prerelease --add-source https://pkgs.dev.azure.com/pleonex/Pleosoft/_packaging/Pleosoft-Preview/nuget/v3/index.json`

#### Localization linter: commands

- `po langtool`: check the translation text of one or more PO files with
  [LanguageTool](https://languagetool.org/) for grammar and typo errors. It
  prints the issues in the console in a CSV file.

Run with the `--help` argument to see the help page with each argument.

#### Localization linter: walkthrough

> [!NOTE]  
> **tl;dr**:
>
> 1. Set up a [LanguageTool](https://languagetool.org/) server.
> 2. Run the linter:
>    - Simple command: `loclinter po langtool -f "name.po" -o "issues.csv"`
>    - Advanced command:
>      `loclinter po langtool -d "Translation/es" -r -o "issues.csv" --markup-regex "(%[ds])" --dict "dictionary.txt" --markup-mapping "mappings.txt"`

Running the program for the first time is a bit tedious as it requires creating
several files. Let's go step by step:

1. Set up a [LanguageTool](https://languagetool.org/) server.

   - _LanguageTool_ requires high resources to check for issues. The free cloud
     version (and even the paid one) are very rate limited. As it's open-source
     you can set up your own instance and run the checks locally.
   - The instructions are for running as a container with the image from
     [meyayl](https://github.com/meyayl/docker-languagetool).

   1. Install a container engine like Docker. On Windows and Mac OS you use
      [Rancher Desktop](https://rancherdesktop.io/)
   2. Create a Docker Compose setup to run the container. For instance
      (documentation [here](https://github.com/meyayl/docker-languagetool)):

      ```yml
      name: "languagetool"
      services:
        server:
          image: meyay/languagetool
          restart: unless-stopped
          ports:
            - 8010:8010
          environment:
            download_ngrams_for_langs: en,es
            langtool_languageModel: /ngrams
            langtool_fasttextModel: /fasttext/lid.176.bin
          volumes:
            - ngrams:/ngrams
            - fasttext:/fasttext

      volumes:
        ngrams:
        fasttext:
      ```

   3. Run `docker compose up -d`. You can stop it with `docker compose stop`.
      Once you don't need the server anymore, you can delete all its data with
      `docker compose down -v`

2. You can now run the linter:
   `loclinter po langtool --file name.po --output issues.csv`
   - The default server is `http://localhost:8010/v2/`. If your server is in
     another computer or public port, use the option `--language-tool <url>`
   - You can run checks on all files from a directory with `--directory <path>`.
     Specify `--recursive` to search inside the subfolders as well.
   - To enable additional rules, use `--picky`.
   - **Keep reading to learn how to tweak the checks and get more accurate
     results.**
3. (Optional) Create a custom dictionary with words to ignore for errors (e.g.,
   specific business terminology, abbreviations, names). Use the option:
   `--dict <path>`.

   - The format is a plain UTF-8 text file with one line per word to ignore.
   - Lines starting with `#` are ignored.
   - Starting and ending spaces are removed.
   - For instance to ignore the word `Articuno`:

     ```plain
     # Pokémons
     Articuno
     ```

4. (Optional) Prepare a regular expression that matches markup codes and
   variable tokens. Any matching text will be ignored by _LanguageTool_. Specify
   the expression with `--markup-regex <regex>`.

   - To match XML tags like `<b>` use: `(</?b>)`
     ([please don't try to parse HTML with regex](https://stackoverflow.com/a/1732454))
   - To match `%d` and `%s` use: `(%[ds])`.
   - To match `{color:51}`, `\\p` and `%s` use: `({[\w:]+}*)|(\\p)|(%[ds])`

5. (Optional) By default, any captured _markup_ word is treat as an invisible
   character. Meaning the checker will not expect any content in its place. This
   means it will report having unnecessary space before the dot for
   `Name: {pokemon:144}.`. You can specify in a markup mapping file an example
   content to replace the detected markup. It will run the check replacing the
   content with the token. Specify the file with: `--markup-mapping <path>`

   - The format is a plain UTF-8 text file with one line per word to ignore.
   - Lines starting with `#` are ignored.
   - Starting and ending spaces are removed.
   - For instance to interpret `{pokemon:144}` as word:

     ```plain
     {pokemon:\d+}=Pikachu
     ```

6. Run the full version of the command:
   `loclinter po langtool -d "Translation/es" -r -o "issues.csv" --markup-regex "(%[ds])" --dict "dictionary.txt" --markup-mapping "mappings.txt"`

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
This will trigger a pipeline that automatically builds, bundles and publishes
the artifacts of the project.
