using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanVM.Core.ISO;

public class ISOCatalog
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [JsonPropertyName("categories")]
    public List<ISOCategory> Categories { get; set; } = new();

    public static async Task<ISOCatalog> LoadEmbeddedAsync()
    {
        var assembly = typeof(ISOCatalog).Assembly;
        var resourceName = "CleanVM.Core.ISO.ISOCatalog.json";
        
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Fallback to file system if embedded resource not found
            var filePath = Path.Combine(
                Path.GetDirectoryName(assembly.Location) ?? "",
                "ISO",
                "ISOCatalog.json"
            );
            
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<ISOCatalog>(json) ?? new ISOCatalog();
            }
            
            return new ISOCatalog();
        }
        
        return await JsonSerializer.DeserializeAsync<ISOCatalog>(stream) ?? new ISOCatalog();
    }

    public static async Task<ISOCatalog> LoadFromUrlAsync(string url, HttpClient httpClient)
    {
        try
        {
            var json = await httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<ISOCatalog>(json) ?? new ISOCatalog();
        }
        catch
        {
            return new ISOCatalog();
        }
    }

    public static ISOCatalog Merge(ISOCatalog embedded, ISOCatalog extended)
    {
        var merged = new ISOCatalog
        {
            Version = string.Compare(embedded.Version, extended.Version) > 0 ? embedded.Version : extended.Version,
            LastUpdated = embedded.LastUpdated > extended.LastUpdated ? embedded.LastUpdated : extended.LastUpdated
        };

        // Merge categories
        var categoryDict = new Dictionary<string, ISOCategory>();
        
        foreach (var category in embedded.Categories)
        {
            categoryDict[category.Id] = category;
        }
        
        foreach (var category in extended.Categories)
        {
            if (categoryDict.TryGetValue(category.Id, out var existing))
            {
                // Merge templates within category
                var templateDict = existing.Templates.ToDictionary(t => t.Id);
                foreach (var template in category.Templates)
                {
                    templateDict[template.Id] = template; // Extended overrides embedded
                }
                existing.Templates = templateDict.Values.ToList();
            }
            else
            {
                categoryDict[category.Id] = category;
            }
        }

        merged.Categories = categoryDict.Values.ToList();
        return merged;
    }
}

public class ISOCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("templates")]
    public List<ISOTemplate> Templates { get; set; } = new();
}

public class ISOTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("architecture")]
    public string Architecture { get; set; } = string.Empty;

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("checksum")]
    public ISOChecksum? Checksum { get; set; }

    [JsonPropertyName("recommendedSpecs")]
    public RecommendedSpecs? RecommendedSpecs { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class ISOChecksum
{
    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "SHA256";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class RecommendedSpecs
{
    [JsonPropertyName("cpuCores")]
    public int CpuCores { get; set; }

    [JsonPropertyName("memoryMB")]
    public int MemoryMB { get; set; }

    [JsonPropertyName("diskGB")]
    public int DiskGB { get; set; }
}
