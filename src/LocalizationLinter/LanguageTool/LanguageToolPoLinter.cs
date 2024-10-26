namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

using System.Text.RegularExpressions;
using PleOps.LanguageTool.Client;
using PleOps.LanguageTool.Client.TextCheck;
using Yarhl.Media.Text;

public class LanguageToolPoLinter
{
    private readonly LanguageToolClient client;

    public LanguageToolPoLinter(LanguageToolClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    public async IAsyncEnumerable<(PoEntry, TextCheckResult)> LintAsync(
        Po po,
        bool picky,
        string? markupRegexText,
        string? markupMappingPath,
        IProgress<PoEntry> progress)
    {
        var checkParameters = new TextCheckParameters {
            Language = po.Header.Language,
            Picky = picky,
        };

        RegexMarkupBuilder? regexBuilder = CreateMarkupBuilder(markupRegexText, markupMappingPath);

        foreach (PoEntry entry in po.Entries) {
            progress.Report(entry);

            if (string.IsNullOrWhiteSpace(entry.Translated)) {
                continue;
            }

            TextCheckResult results;
            if (regexBuilder == null) {
                results = await client.CheckPlainTextAsync(entry.Translated, checkParameters);
            } else {
                var markup = regexBuilder.Build(entry.Translated);
                results = await client.CheckMarkupTextAsync(markup, checkParameters);
            }


            yield return (entry, results);
        }
    }

    private static RegexMarkupBuilder? CreateMarkupBuilder(string? markupRegexText, string? markupMappingPath)
    {
        if (string.IsNullOrEmpty(markupRegexText)) {
            return null;
        }

        Dictionary<Regex, string> mapping = [];
        if (!string.IsNullOrEmpty(markupMappingPath)) {
            mapping = File.ReadAllLines(markupMappingPath)
                .Select(l => l.Trim().Split(['='], 2))
                .Where(l => l.Length == 2 && !l[0].StartsWith('#'))
                .ToDictionary(l => new Regex(l[0]), l => l[1]);
        }

        var regex = new Regex(markupRegexText);
        return new RegexMarkupBuilder(regex, mapping);
    }
}
