namespace PleOps.Il28n.Formats.Resx;

using Yarhl.FileFormat;
using Yarhl.Media.Text;

public class Resx2Po : IConverter<LocalizedResxCatalog, Po>
{
    private readonly string projectId;
    private readonly string reporter;
    private readonly LocalizedResxCatalog sourceCatalog;

    public Resx2Po(string projectId, string reporter, LocalizedResxCatalog sourceCatalog)
    {
        ArgumentNullException.ThrowIfNull(projectId);
        ArgumentNullException.ThrowIfNull(reporter);
        ArgumentNullException.ThrowIfNull(sourceCatalog);

        this.projectId = projectId;
        this.reporter = reporter;
        this.sourceCatalog = sourceCatalog;
    }

    public Po Convert(LocalizedResxCatalog targetCatalog)
    {
        var header = new PoHeader(projectId, reporter, targetCatalog.Language);
        var po = new Po(header);

        foreach (LocalizedResxMessage sourceEntry in sourceCatalog.Messages) {
            LocalizedResxMessage? targetEntry = targetCatalog.Messages
                .FirstOrDefault(m => m.Id == sourceEntry.Id);

            var poEntry = new PoEntry(sourceEntry.Value!) {
                Context = sourceEntry.Id,
                Translated = targetEntry?.Value ?? string.Empty,
                ExtractedComments = sourceEntry.Comment ?? "",
                TranslatorComment = targetEntry?.Comment ?? "",
            };
            po.Add(poEntry);
        }

        return po;
    }
}
