namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

using System.Collections.ObjectModel;
using PleOps.LanguageTool.Client;
using PleOps.LanguageTool.Client.Check;
using Yarhl.Media.Text;

public class LanguageToolPoLinter
{
    private readonly LanguageToolClient client;

    public LanguageToolPoLinter(LanguageToolClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    public async IAsyncEnumerable<(PoEntry, ReadOnlyCollection<CheckPostResponse_matches>)> LintAsync(Po po, IProgress<PoEntry> progress)
    {
        string language = po.Header.Language;
        foreach (PoEntry entry in po.Entries) {
            progress.Report(entry);

            if (string.IsNullOrWhiteSpace(entry.Translated)) {
                continue;
            }

            var results = await client.CheckTextAsync(entry.Translated, language, true);
            yield return (entry, results);
        }
    }
}
