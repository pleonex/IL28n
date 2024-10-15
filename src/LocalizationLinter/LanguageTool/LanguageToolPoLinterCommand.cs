namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

using System.ComponentModel;
using System.Threading.Tasks;
using PleOps.LanguageTool.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using Yarhl.IO;
using Yarhl.Media.Text;

[Description("Use LanguageTool to run spell and grammar checks on a PO file")]
internal class LanguageToolPoLinterCommand : AsyncCommand<LanguageToolPoLinterCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[INPUT-PO]")]
        [Description("Path to the PO file to check for spell and grammar")]
        public required string InputPoPath { get; set; }

        [CommandArgument(1, "[OUTPUT-CSV]")]
        [Description("Output CSV file with reported issues")]
        public required string CsvOutputPath { get; set; }

        [CommandOption("--language-tool")]
        [DefaultValue("http://localhost:8010/v2/")]
        [Description("URL to the LanguageTool server")]
        public required string LanguageToolUrl { get; set; }

        [CommandOption("--dict")]
        [Description("Path to the file with words to ignore in checks")]
        public string? UserDictionaryPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLineInterpolated($"LanguageTool server: [blue]{settings.LanguageToolUrl}[/]");
        var languageToolClient = LanguageToolClientFactory.Create(settings.LanguageToolUrl);

        if (!string.IsNullOrWhiteSpace(settings.UserDictionaryPath)) {
            AnsiConsole.MarkupLineInterpolated($"Loading user dictionary: [blue]{settings.UserDictionaryPath}[/]");
            languageToolClient.AddUserDictionary(settings.UserDictionaryPath);
        }

        AnsiConsole.MarkupLineInterpolated($"Loading input PO: [blue]{settings.InputPoPath}[/]");
        Po po;
        using (var inputStream = new BinaryFormat(settings.InputPoPath, FileOpenMode.Read)) {
            po = new Binary2Po().Convert(inputStream);
        }
        AnsiConsole.MarkupLineInterpolated($"PO with [bold blue]{po.Entries.Count}[/] messages");

        AnsiConsole.MarkupLineInterpolated($"Writing issues into CSV: [blue]{settings.CsvOutputPath}[/]");
        var csvReporter = new LanguageToolCsvIssueSerializer(settings.CsvOutputPath);

        AnsiConsole.MarkupLine("[bold]Starting linting...[/]");
        var linter = new LanguageToolPoLinter(languageToolClient);

        int issueCount = 0;
        var progress = new Progress<PoEntry>(
            e => AnsiConsole.MarkupLineInterpolated($"[[{e.Context}]] '[italic]{e.Translated}[/]'"));
        await foreach (var results in linter.LintAsync(po, progress)) {
            PoEntry entry = results.Item1;
            var matches = results.Item2;

            csvReporter.AddIssues(entry, matches);

            var tree = new Tree("");
            foreach (var match in matches) {
                string bad = entry.Translated.Substring(match.Offset!.Value, match.Length!.Value);
                string suggestions = string.Join(", ", match.Replacements!.Select(r => r.Value));
                tree.AddNode($"[red]{match.Message}[/]: [red]{bad}[/] -> [blue]{suggestions}[/]");
                issueCount++;
            }

            AnsiConsole.Write(tree);
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Done![/] Found: [red]{issueCount}[/] issues");
        return 0;
    }
}
