namespace PleOps.Il28n.ResxConverter.Resx;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using System.Xml.Linq;

public static class ResxLocalizationManager
{
    public static LocalizedResxCatalog ReadResxFile(string resxPath)
    {
        string name = Path.GetFileNameWithoutExtension(resxPath);
        string language = "";
        int langSeparator = name.LastIndexOf('.');
        if (langSeparator != -1) {
            language = name.Substring(langSeparator + 1);
            name = name.Substring(0, langSeparator);
        }

        var catalog = new LocalizedResxCatalog { Name = name, Language = language };
        var xml = XDocument.Load(resxPath);

        var xmlMessages = xml.Root?.Elements("data") ?? throw new FormatException("Empty XML");
        foreach (XElement xmlMessage in xmlMessages) {
            var message = new LocalizedResxMessage {
                Id = xmlMessage.Attribute("name")?.Value ?? throw new FormatException("Missing name attribute"),
                Value = xmlMessage.Element("value")?.Value,
                Comment = xmlMessage.Element("comment")?.Value,
            };
            catalog.Messages.Add(message);
        }

        return catalog;
    }

    [SupportedOSPlatform("windows")]
    public static LocalizedResxCatalog ReadBinaryResource(string binaryResourcePath)
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

    public static LocalizedResxCatalog ReadEmbeddedResource(Assembly resourceAssembly, string resourceName, CultureInfo culture)
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
