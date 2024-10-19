namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

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
        IProgress<PoEntry> progress)
    {
        string language = po.Header.Language;
        foreach (PoEntry entry in po.Entries) {
            progress.Report(entry);

            if (string.IsNullOrWhiteSpace(entry.Translated)) {
                continue;
            }

            TextCheckResult results = await client.CheckPlainTextAsync(entry.Translated, language, true);
            yield return (entry, results);
        }
    }
}
