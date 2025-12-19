using CleanVM.Core.ISO;
using CleanVM.Core.Interfaces;

namespace CleanVM.CLI;

public static class ISOCommands
{
    public static async Task ListCatalogAsync(IISODownloader downloader)
    {
        Console.WriteLine("📚 CleanVM ISO Catalog\n");
        
        var catalog = await ((ISODownloadManager)downloader).GetCatalogAsync();
        if (catalog == null)
        {
            Console.WriteLine("❌ Failed to load catalog");
            return;
        }

        Console.WriteLine($"Version: {catalog.Version}");
        Console.WriteLine($"Last Updated: {catalog.LastUpdated:yyyy-MM-dd}\n");

        foreach (var category in catalog.Categories)
        {
            Console.WriteLine($"{category.Icon} {category.Name}");
            Console.WriteLine(new string('─', 50));
            
            foreach (var template in category.Templates)
            {
                var sizeGB = template.SizeBytes / 1_000_000_000.0;
                Console.WriteLine($"  • {template.Name}");
                Console.WriteLine($"    ID: {template.Id}");
                Console.WriteLine($"    Version: {template.Version} | Arch: {template.Architecture}");
                Console.WriteLine($"    Size: {sizeGB:F1} GB");
                if (!string.IsNullOrEmpty(template.Notes))
                {
                    Console.WriteLine($"    Note: {template.Notes}");
                }
                Console.WriteLine();
            }
        }
    }

    public static async Task DownloadISOAsync(IISODownloader downloader, string templateId, string outputDir)
    {
        var templates = await downloader.GetAvailableTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Id == templateId);
        
        if (template == null)
        {
            Console.WriteLine($"❌ Template '{templateId}' not found");
            Console.WriteLine("\nAvailable templates:");
            foreach (var t in templates)
            {
                Console.WriteLine($"  • {t.Id} - {t.Name}");
            }
            return;
        }

        Console.WriteLine($"📥 Downloading {template.Name}");
        Console.WriteLine($"Version: {template.Version}");
        Console.WriteLine($"Size: {template.SizeBytes / 1_000_000_000.0:F1} GB");
        Console.WriteLine($"URL: {template.DownloadUrl}\n");

        var progress = new Progress<DownloadProgress>(p =>
        {
            var percent = p.PercentComplete;
            var downloadedGB = p.BytesDownloaded / 1_000_000_000.0;
            var totalGB = p.TotalBytes / 1_000_000_000.0;
            
            Console.Write($"\r[{new string('█', (int)(percent / 5))}{new string('░', 20 - (int)(percent / 5))}] {percent:F1}% ({downloadedGB:F2}/{totalGB:F2} GB)");
        });

        try
        {
            var manager = (ISODownloadManager)downloader;
            var downloadedPath = await manager.DownloadTemplateAsync(templateId, progress);
            
            Console.WriteLine($"\n\n✅ Download complete!");
            Console.WriteLine($"📁 Saved to: {downloadedPath}");
            
            if (template.Checksum != null && !string.IsNullOrEmpty(template.Checksum.Value))
            {
                Console.WriteLine("✓ Checksum verified");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n❌ Download failed: {ex.Message}");
        }
    }

    public static async Task ShowTemplateDetailsAsync(IISODownloader downloader, string templateId)
    {
        var templates = await downloader.GetAvailableTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Id == templateId);
        
        if (template == null)
        {
            Console.WriteLine($"❌ Template '{templateId}' not found");
            return;
        }

        Console.WriteLine($"\n{template.Name}");
        Console.WriteLine(new string('=', template.Name.Length));
        Console.WriteLine($"ID: {template.Id}");
        Console.WriteLine($"Description: {template.Description}");
        Console.WriteLine($"Version: {template.Version}");
        Console.WriteLine($"Architecture: {template.Architecture}");
        Console.WriteLine($"Size: {template.SizeBytes / 1_000_000_000.0:F2} GB");
        Console.WriteLine($"Download URL: {template.DownloadUrl}");
        
        if (template.Checksum != null)
        {
            Console.WriteLine($"Checksum ({template.Checksum.Algorithm}): {template.Checksum.Value}");
        }
        
        if (template.RecommendedSpecs != null)
        {
            Console.WriteLine("\nRecommended Specs:");
            Console.WriteLine($"  CPU Cores: {template.RecommendedSpecs.CpuCores}");
            Console.WriteLine($"  Memory: {template.RecommendedSpecs.MemoryMB} MB");
            Console.WriteLine($"  Disk: {template.RecommendedSpecs.DiskGB} GB");
        }
        
        if (!string.IsNullOrEmpty(template.Notes))
        {
            Console.WriteLine($"\nNotes: {template.Notes}");
        }
    }
}
