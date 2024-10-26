namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

using System.ComponentModel;
using System.Threading.Tasks;
using PleOps.LanguageTool.Client;
using PleOps.LanguageTool.Client.TextCheck;
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

        [CommandOption("--markup-regex")]
        [Description("Regular expression to mark and exclude markup codes")]
        public string? MarkupRegex { get; set; }

        [CommandOption("--markup-mapping")]
        [Description("File with markup mappings")]
        public string? MarkupMappingPath { get; set; }

        [CommandOption("--picky")]
        [Description("Run the checks in picky mode")]
        public bool Picky { get; set; }

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
        var clientOptions = new LanguageToolClientOptions { BaseAddress = settings.LanguageToolUrl };
        clientOptions.RetryOptions.MaxRetry = 3;
        clientOptions.RetryOptions.Delay = 1;
        LanguageToolClient languageToolClient = LanguageToolClientFactory.Create(clientOptions);

        if (!string.IsNullOrWhiteSpace(settings.UserDictionaryPath)) {
            AnsiConsole.MarkupLineInterpolated($"Loading user dictionary: [blue]{settings.UserDictionaryPath}[/]");
            languageToolClient.AddUserDictionary(settings.UserDictionaryPath);
        }

        AnsiConsole.MarkupLineInterpolated($"Output CSV file: [blue]{settings.CsvOutputPath}[/]");
        var reporter = new LanguageToolCsvIssueSerializer(settings.CsvOutputPath);

        var linter = new LanguageToolPoLinter(languageToolClient);

        int issuesCount = 0;
        if (!string.IsNullOrWhiteSpace(settings.InputFilePath)) {
            issuesCount = await LintPo(settings, linter, reporter, settings.InputFilePath);
        } else if (!string.IsNullOrWhiteSpace(settings.InputDirectoryPath)) {
            IEnumerable<string> inputFiles = Directory.EnumerateFiles(
                settings.InputDirectoryPath,
                "*.po",
                settings.RecursiveDirectorySearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (string inputFile in inputFiles) {
                issuesCount += await LintPo(settings, linter, reporter, inputFile);
            }
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Done![/] Found: [red]{issuesCount}[/] issues");
        return 0;
    }

    private static async Task<int> LintPo(
        Settings settings,
        LanguageToolPoLinter linter,
        LanguageToolCsvIssueSerializer reporter,
        string poPath)
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

        var results = linter.LintAsync(po, settings.Picky, settings.MarkupRegex, settings.MarkupMappingPath, progress);
        await foreach ((PoEntry entry, TextCheckResult checkResult) in results) {
            reporter.ReportIssues(name, entry.Context, checkResult);

            var tree = new Tree("");
            foreach (TextCheckMatch match in checkResult.Matches) {
                string suggestions = string.Join(", ", match.Replacements);
                _ = tree.AddNode($"[red]{match.Message}[/]: [red]{match.TextMatch}[/] -> [blue]{suggestions}[/]");
                issuesCount++;
            }

            AnsiConsole.Write(tree);
        }

        return issuesCount;
    }
}
