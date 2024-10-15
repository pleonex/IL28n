namespace PleOps.Il28n.ResxConverter.Commands;

using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PleOps.Il28n.Formats.Resx;
using Spectre.Console;
using Spectre.Console.Cli;
using Yarhl.IO;
using Yarhl.Media.Text;

[Description("Convert monolingual RESX files into PO format")]
internal class Resx2PoCommand : AsyncCommand<Resx2PoCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-f|--file")]
        [Description("Path to the default culture .resx file")]
        public string? InputFilePath { get; set; }

        [CommandOption("-d|--directory")]
        [Description("Path to a directory the .resx files")]
        public string? InputDirectoryPath { get; set; }

        [CommandOption("-r|--recursive")]
        [Description("Search .resx files in the directory subfolders")]
        public bool RecursiveDirectorySearch { get; set; }

        [CommandOption("--resx-lang")]
        [Description("Language code used in the name of the localized .resx files")]
        public string? ResxLanguage { get; set; }

        [CommandOption("-o|--output")]
        [Description("Path to the output directory to generate the PO file(s)")]
        public required string OutputPath { get; set; }

        [CommandOption("-p|--project-id")]
        [Description("ID of the software project to include in the PO header")]
        public required string ProjectId { get; set; }

        [CommandOption("--reporter")]
        [Description("Email or address to contact to report issues in the translation files")]
        public required string Reporter { get; set; }

        [CommandOption("--po-lang")]
        [Description("New language code to set in the PO if different to the resx language")]
        public string? PoLanguage { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(InputFilePath) && string.IsNullOrWhiteSpace(InputDirectoryPath)) {
                return ValidationResult.Error("Either an input file or directory must be specified");
            }

            if (!string.IsNullOrWhiteSpace(InputFilePath) && !string.IsNullOrWhiteSpace(InputDirectoryPath)) {
                return ValidationResult.Error("An input file and directory cannot be specified at the same time.");
            }

            if (!string.IsNullOrWhiteSpace(InputFilePath) && !File.Exists(InputFilePath)) {
                return ValidationResult.Error("The input file does not exist");
            }

            if (!string.IsNullOrWhiteSpace(InputDirectoryPath) && !Directory.Exists(InputDirectoryPath)) {
                return ValidationResult.Error("The input directory does not exist");
            }

            if (RecursiveDirectorySearch && string.IsNullOrWhiteSpace(InputDirectoryPath)) {
                return ValidationResult.Error("Recursive flag is only valid when passing a directory input");
            }

            if (string.IsNullOrWhiteSpace(ResxLanguage) && string.IsNullOrWhiteSpace(PoLanguage)) {
                return ValidationResult.Error(
                    "ResxLanguage is missing. " +
                    "In that case the default culture will be used and PoLanguage must be specified");
            }

            if (string.IsNullOrWhiteSpace(OutputPath)) {
                return ValidationResult.Error("The output path must be specified");
            }

            if (string.IsNullOrWhiteSpace(ProjectId) || string.IsNullOrWhiteSpace(Reporter)) {
                return ValidationResult.Error($"{nameof(ProjectId)} and {nameof(Reporter)} are mandatory");
            }

            return base.Validate();
        }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        int fileCount = 0;
        if (!string.IsNullOrEmpty(settings.InputFilePath)) {
            var component = MatchFile(settings.InputFilePath, settings.InputFilePath, settings.ResxLanguage);
            ConvertResxFile(component, settings);
            fileCount = 1;
        } else if (!string.IsNullOrEmpty(settings.InputDirectoryPath)) {
            var components = MatchFiles(
                settings.InputDirectoryPath,
                settings.RecursiveDirectorySearch,
                settings.ResxLanguage);
            foreach (var component in components) {
                ConvertResxFile(component, settings);
                fileCount++;
            }
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Done![/] Converted [green]{fileCount}[/] files");
        return Task.FromResult(0);
    }

    private static IEnumerable<ResxComponentInfo> MatchFiles(string rootPath, bool recursive, string? resxLanguage)
    {
        IEnumerable<string> inputFiles = Directory.EnumerateFiles(
            rootPath,
            "*.resx",
            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .OrderBy(f => f.Length);

        List<string> readComponents = new();
        foreach (string inputFile in inputFiles) {
            string currentDir = Path.GetDirectoryName(inputFile)!;
            string filename = Path.GetFileNameWithoutExtension(inputFile);
            string fullComponentName = Path.Combine(currentDir, filename);
            if (filename.Contains('.')) {
                string filenameWithoutLastSegment = Path.GetFileNameWithoutExtension(filename);
                string simulatedComponentName = Path.Combine(currentDir, filenameWithoutLastSegment);
                if (readComponents.Contains(simulatedComponentName, StringComparer.InvariantCulture)) {
                    // skip language file
                    continue;
                }
            }

            readComponents.Add(fullComponentName);
            yield return MatchFile(rootPath, inputFile, resxLanguage);
        }
    }

    private static ResxComponentInfo MatchFile(string rootPath, string baseFile, string? resxLanguage)
    {
        string resxDir = Path.GetDirectoryName(baseFile)!;
        string relativePath = Path.GetRelativePath(rootPath, resxDir);
        if (relativePath == ".") {
            relativePath = "";
        }

        string resxName = Path.GetFileNameWithoutExtension(baseFile);

        string localizedResxName = string.IsNullOrWhiteSpace(resxLanguage)
            ? resxName
            : $"{resxName}.{resxLanguage}";
        string localizedResxPath = Path.Combine(resxDir, localizedResxName + ".resx");

        return new ResxComponentInfo(baseFile, localizedResxPath, relativePath);
    }

    private static void ConvertResxFile(ResxComponentInfo component, Settings settings)
    {
        string name = Path.GetFileNameWithoutExtension(component.BaseResxPath);
        string outputPath = Path.Combine(settings.OutputPath, component.RelativePath, $"{name}.po");

        AnsiConsole.MarkupLineInterpolated($"Converting [blue]{component.RelativePath}/{name}[/]");
        LocalizedResxCatalog baseResx = ResxLocalizationManager.ReadResxFile(component.BaseResxPath);
        LocalizedResxCatalog translationResx = ResxLocalizationManager.ReadResxFile(component.LocalizedResxPath);

        if (!string.IsNullOrWhiteSpace(settings.PoLanguage)) {
            translationResx.Language = settings.PoLanguage;
        }

        var converter = new Resx2Po(settings.ProjectId, settings.Reporter, baseResx);
        Po po = converter.Convert(translationResx);

        using BinaryFormat binaryPo = new Po2Binary().Convert(po);
        binaryPo.Stream.WriteTo(outputPath);
    }

    private sealed record ResxComponentInfo(
        string BaseResxPath,
        string LocalizedResxPath,
        string RelativePath);
}
