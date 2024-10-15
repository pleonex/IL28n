namespace PleOps.Il28n.Formats.Resx;

using System.Collections.ObjectModel;

public class LocalizedResxCatalog
{
    public required string Name { get; init; }

    public required string Language { get; set; }

    public Collection<LocalizedResxMessage> Messages { get; init; } = [];
}
