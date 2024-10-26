namespace PleOps.Il28n.LocalizationLinter.LanguageTool;

using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using PleOps.LanguageTool.Client.TextCheck;

public class LanguageToolCsvIssueSerializer : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter outputStream;
    private readonly CsvWriter writer;

    public LanguageToolCsvIssueSerializer(string filePath)
    {
        string dirPath = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
        _ = Directory.CreateDirectory(dirPath);

        var streamOptions = new FileStreamOptions {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
        };
        outputStream = new StreamWriter(filePath, Encoding.UTF8, streamOptions);
        writer = new CsvWriter(outputStream, CultureInfo.InvariantCulture);
    }

    public bool IsDisposed { get; private set; }

    public void ReportIssues(string componentName, string messageId, TextCheckResult checkResult)
    {
        IEnumerable<CsvEntry> entries = checkResult
            .Matches
            .Select(i => MapToCsvEntry(componentName, messageId, i));
        writer.WriteRecords(entries);
        writer.Flush();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await outputStream.DisposeAsync();
        await writer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) {
            return;
        }

        if (disposing) {
            outputStream.Dispose();
            writer.Dispose();
        }

        IsDisposed = true;
    }

    private static CsvEntry MapToCsvEntry(string componentName, string messageId, TextCheckMatch issue)
    {
        return new CsvEntry {
            ComponentName = componentName,
            Id = messageId,
            Translation = issue.Sentence,
            AffectedText = issue.TextMatch,
            IssueMessage = issue.Message!,
            Suggestions = string.Join(", ", issue.Replacements),
        };
    }

    public sealed class CsvEntry
    {
        [Index(0)]
        [Name("Component")]
        public required string ComponentName { get; set; }

        [Index(1)]
        [Name("ID")]
        public required string Id { get; set; }

        [Index(2)]
        [Name("Translation")]
        public required string Translation { get; set; }

        [Index(3)]
        [Name("Translation issue")]
        public required string AffectedText { get; set; }

        [Index(4)]
        [Name("Issue message")]
        public required string IssueMessage { get; set; }

        [Index(5)]
        [Name("Suggestions")]
        public required string Suggestions { get; set; }
    }
}
