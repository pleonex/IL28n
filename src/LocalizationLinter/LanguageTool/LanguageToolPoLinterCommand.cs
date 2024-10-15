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
        [CommandOption("-f|--file")]
        [Description("Path to a single PO file to check for spell and grammar")]
        public string? InputFilePath { get; set; }

        [CommandOption("-d|--directory")]
        [Description("Path to a directory containing PO files to check for spell and grammar")]
        public string? InputDirectoryPath { get; set; }

        [CommandOption("-r|--recursive")]
        [Description("Search PO files in the directory subfolders")]
        public bool RecursiveDirectorySearch { get; set; }

        [CommandOption("-o|--output")]
        [Description("Output CSV file with reported issues")]
        public required string CsvOutputPath { get; set; }

        [CommandOption("--language-tool")]
        [DefaultValue("http://localhost:8010/v2/")]
        [Description("URL to the LanguageTool server")]
        public required string LanguageToolUrl { get; set; }

        [CommandOption("--dict")]
        [Description("Path to the file with words to ignore in checks")]
        public string? UserDictionaryPath { get; set; }

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

            if (string.IsNullOrWhiteSpace(CsvOutputPath)) {
                return ValidationResult.Error("The output CSV path must be specified");
            }

            return base.Validate();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLineInterpolated($"LanguageTool server: [blue]{settings.LanguageToolUrl}[/]");
        var languageToolClient = LanguageToolClientFactory.Create(settings.LanguageToolUrl);

        if (!string.IsNullOrWhiteSpace(settings.UserDictionaryPath)) {
            AnsiConsole.MarkupLineInterpolated($"Loading user dictionary: [blue]{settings.UserDictionaryPath}[/]");
            languageToolClient.AddUserDictionary(settings.UserDictionaryPath);
        }

        AnsiConsole.MarkupLineInterpolated($"Output CSV file: [blue]{settings.CsvOutputPath}[/]");
        var reporter = new LanguageToolCsvIssueSerializer(settings.CsvOutputPath);

        var linter = new LanguageToolPoLinter(languageToolClient);

        int issuesCount = 0;
        if (!string.IsNullOrWhiteSpace(settings.InputFilePath)) {
            issuesCount = await LintPo(linter, reporter, settings.InputFilePath);
        } else if (!string.IsNullOrWhiteSpace(settings.InputDirectoryPath)) {
            IEnumerable<string> inputFiles = Directory.EnumerateFiles(
                settings.InputDirectoryPath,
                "*.po",
                settings.RecursiveDirectorySearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (string inputFile in inputFiles) {
                issuesCount += await LintPo(linter, reporter, inputFile);
            }
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Done![/] Found: [red]{issuesCount}[/] issues");
        return 0;
    }

    private static async Task<int> LintPo(LanguageToolPoLinter linter, LanguageToolCsvIssueSerializer reporter, string poPath)
    {
        string name = Path.GetFileNameWithoutExtension(poPath);

        Po po;
        using (var inputStream = new BinaryFormat(poPath, FileOpenMode.Read)) {
            po = new Binary2Po().Convert(inputStream);
        }

        AnsiConsole.Write(new Rule($"[blue]{name}[/]: {po.Entries.Count}"));

        int issuesCount = 0;
        var progress = new Progress<PoEntry>(
            e => AnsiConsole.MarkupLineInterpolated($"[[{e.Context}]] '[italic]{e.Translated}[/]'"));

        await foreach (var results in linter.LintAsync(po, progress)) {
            reporter.ReportIssues(name, results.Item1.Context, results.Item2);

            var tree = new Tree("");
            foreach (var match in results.Item2) {
                string bad = match.Sentence!.Substring(match.Offset!.Value, match.Length!.Value);
                string suggestions = string.Join(", ", match.Replacements!.Select(r => r.Value));
                tree.AddNode($"[red]{match.Message}[/]: [red]{bad}[/] -> [blue]{suggestions}[/]");
                issuesCount++;
            }

            AnsiConsole.Write(tree);
        }

        return issuesCount;
    }
}
