namespace PleOps.Il28n.ResxConverter.Commands;

using System.ComponentModel;
using System.Threading.Tasks;
using PleOps.Il28n.Formats.Resx;
using Spectre.Console;
using Spectre.Console.Cli;
using Yarhl.IO;
using Yarhl.Media.Text;

[Description("Convert a RESX file into PO format")]
internal class Resx2PoCommand : AsyncCommand<Resx2PoCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[BASE_RESX_PATH]")]
        [Description("Path to the default culture .resx file")]
        public required string BaseResxPath { get; set; }

        [CommandArgument(1, "[LANGUAGE]")]
        [Description("Language code for the target culture")]
        public required string LanguageCode { get; set; }

        [CommandArgument(2, "[OUTPUT_PO_PATH]")]
        [Description("Path to the output PO file")]
        public required string OutputPoPath { get; set; }

        [CommandOption("-p|--project-id")]
        [Description("ID of the software project to include in the PO header")]
        public required string ProjectId { get; set; }

        [CommandOption("-r|--reporter")]
        [Description("Email or address to contact to report issues in the translation files")]
        public required string Reporter { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(ProjectId) || string.IsNullOrWhiteSpace(Reporter)) {
                return ValidationResult.Error($"{nameof(ProjectId)} and {nameof(Reporter)} are mandatory");
            }

            return base.Validate();
        }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        string resxDir = Path.GetDirectoryName(settings.BaseResxPath)!;
        string resxName = Path.GetFileNameWithoutExtension(settings.BaseResxPath);
        string targetResxPath = Path.Combine(resxDir, $"{resxName}.{settings.LanguageCode}.resx");
        if (!File.Exists(targetResxPath)) {
            AnsiConsole.MarkupLineInterpolated($"[red]Cannot find target RESX file[/]: {targetResxPath}");
            return Task.FromResult(1);
        }

        AnsiConsole.MarkupLineInterpolated($"Reading RESX base file: [blue]{settings.BaseResxPath}[/]");
        LocalizedResxCatalog baseResx = ResxLocalizationManager.ReadResxFile(settings.BaseResxPath);

        AnsiConsole.MarkupLineInterpolated($"Reading RESX localized file: [blue]{targetResxPath}[/]");
        LocalizedResxCatalog translationResx = ResxLocalizationManager.ReadResxFile(targetResxPath);

        AnsiConsole.MarkupLineInterpolated($"Converting into PO format");
        var converter = new Resx2Po(settings.ProjectId, settings.Reporter, baseResx);
        Po po = converter.Convert(translationResx);

        AnsiConsole.MarkupLineInterpolated($"Writing PO in [blue]{settings.OutputPoPath}[/]");
        using BinaryFormat binaryPo = new Po2Binary().Convert(po);
        binaryPo.Stream.WriteTo(settings.OutputPoPath);

        AnsiConsole.MarkupLine("[green]Done![/]");
        return Task.FromResult(0);
    }
}
