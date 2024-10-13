namespace PleOps.Il28n.ResxConverter.Resx;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;

public class ResxLocalizationManager
{
    public LocalizedResxCatalog ReadResxFile(string resxPath)
    {
        throw new NotImplementedException();
    }

    [SupportedOSPlatform("windows")]
    public LocalizedResxCatalog ReadBinaryResource(string binaryResourcePath)
    {
        using var reader = new ResourceReader(binaryResourcePath);
        IDictionaryEnumerator dict = reader.GetEnumerator();

        string name = Path.GetFileNameWithoutExtension(binaryResourcePath);
        var catalog = new LocalizedResxCatalog { Name = name, Language = "" };

        while (dict.MoveNext()) {
            var message = new LocalizedResxMessage {
                Id = dict.Key.ToString()!,
                Value = dict.Value?.ToString(),
            };
            catalog.Messages.Add(message);
        }

        return catalog;
    }

    public LocalizedResxCatalog ReadEmbeddedResource(Assembly resourceAssembly, string resourceName, CultureInfo culture)
    {
        var catalog = new LocalizedResxCatalog {
            Name = resourceName,
            Language = culture.Name,
        };

        var resourceManager = new ResourceManager(resourceName, resourceAssembly);
        using ResourceSet resourceSet = resourceManager.GetResourceSet(culture, true, false)
            ?? throw new InvalidOperationException("Missing resource");

        foreach (DictionaryEntry entry in resourceSet) {
            var message = new LocalizedResxMessage { Id = entry.Key.ToString()!, Value = entry.Value?.ToString() };
            catalog.Messages.Add(message);
        }

        return catalog;
    }
}
