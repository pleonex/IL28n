namespace PleOps.Il28n.ResxConverter.Resx;

using System.Collections.ObjectModel;

public class LocalizedResxCatalog
{
    public required string Name { get; init; }

    public required string Language { get; init; }

    public Collection<LocalizedResxMessage> Messages { get; init; } = [];
}
