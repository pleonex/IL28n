namespace PleOps.Il28n.ResxConverter.Commands;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PleOps.Il28n.ResxConverter.Resx;
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

        [CommandArgument(1, "[LOCALIZED_RESX_PATH]")]
        [Description("Path to the .resx file for a specific culture")]
        public required string LocalizedResxPath { get; set; }

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
        LocalizedResxCatalog baseResx = ResxLocalizationManager.ReadResxFile(settings.BaseResxPath);
        LocalizedResxCatalog translationResx = ResxLocalizationManager.ReadResxFile(settings.LocalizedResxPath);

        var converter = new Resx2Po(settings.ProjectId, settings.Reporter, baseResx);
        Po po = converter.Convert(translationResx);

        using BinaryFormat binaryPo = new Po2Binary().Convert(po);
        binaryPo.Stream.WriteTo(settings.OutputPoPath);

        return Task.FromResult(0);
    }
}
