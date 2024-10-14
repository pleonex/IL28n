namespace PleOps.Il28n.ResxConverter.Resx;

using Yarhl.FileFormat;
using Yarhl.Media.Text;

public class Resx2Pot : IConverter<LocalizedResxCatalog, Po>
{
    private readonly string projectId;
    private readonly string reporter;

    public Resx2Pot(string projectId, string reporter)
    {
        ArgumentNullException.ThrowIfNull(projectId);
        ArgumentNullException.ThrowIfNull(reporter);

        this.projectId = projectId;
        this.reporter = reporter;
    }

     public Po Convert(LocalizedResxCatalog source)
    {
        var header = new PoHeader(projectId, reporter, source.Language);
        var po = new Po(header);

        foreach (LocalizedResxMessage sourceEntry in source.Messages) {
            var poEntry = new PoEntry(sourceEntry.Value!) {
                Context = sourceEntry.Id,
            };
            po.Add(poEntry);
        }

        return po;
    }
}
