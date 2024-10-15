namespace PleOps.Il28n.Formats.Resx;

public class LocalizedResxMessage
{
    public required string Id { get; init; }

    public string? Value { get; set; }

    public string? Comment { get; set; }
}
