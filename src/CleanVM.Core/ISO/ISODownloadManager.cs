using System.Security.Cryptography;
using CleanVM.Core.Interfaces;

namespace CleanVM.Core.ISO;

public class ISODownloadManager : IISODownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _downloadDirectory;
    private readonly string _extendedCatalogUrl;
    private ISOCatalog? _catalog;

    public ISODownloadManager(
        HttpClient httpClient,
        string downloadDirectory,
        string? extendedCatalogUrl = null)
    {
        _httpClient = httpClient;
        _downloadDirectory = downloadDirectory;
        _extendedCatalogUrl = extendedCatalogUrl ?? "https://raw.githubusercontent.com/barrersoftware/CleanVM/master/catalog.json";
        
        Directory.CreateDirectory(_downloadDirectory);
    }

    public async Task<IEnumerable<ISOTemplate>> GetAvailableTemplatesAsync()
    {
        if (_catalog == null)
        {
            await LoadCatalogAsync();
        }

        return _catalog?.Categories.SelectMany(c => c.Templates) ?? Enumerable.Empty<ISOTemplate>();
    }

    private async Task LoadCatalogAsync()
    {
        // Load embedded catalog first (always available)
        var embeddedCatalog = await ISOCatalog.LoadEmbeddedAsync();

        // Try to fetch extended catalog (best effort)
        ISOCatalog? extendedCatalog = null;
        try
        {
            extendedCatalog = await ISOCatalog.LoadFromUrlAsync(_extendedCatalogUrl, _httpClient);
        }
        catch
        {
            // Extended catalog fetch failed, use embedded only
        }

        // Merge catalogs
        _catalog = extendedCatalog != null 
            ? ISOCatalog.Merge(embeddedCatalog, extendedCatalog)
            : embeddedCatalog;
    }

    public async Task<string> DownloadAsync(string url, IProgress<DownloadProgress>? progress = null)
    {
        // Generate filename from URL
        var uri = new Uri(url);
        var filename = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrEmpty(filename) || !filename.EndsWith(".iso", StringComparison.OrdinalIgnoreCase))
        {
            filename = $"download-{DateTime.UtcNow:yyyyMMdd-HHmmss}.iso";
        }

        var destinationPath = Path.Combine(_downloadDirectory, filename);

        // Check if already downloaded
        if (File.Exists(destinationPath))
        {
            var fileInfo = new FileInfo(destinationPath);
            progress?.Report(new DownloadProgress
            {
                BytesDownloaded = fileInfo.Length,
                TotalBytes = fileInfo.Length
            });
            return destinationPath;
        }

        // Download with progress tracking
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var bytesDownloaded = 0L;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            bytesDownloaded += bytesRead;

            progress?.Report(new DownloadProgress
            {
                BytesDownloaded = bytesDownloaded,
                TotalBytes = totalBytes
            });
        }

        return destinationPath;
    }

    public async Task<string> DownloadTemplateAsync(string templateId, IProgress<DownloadProgress>? progress = null)
    {
        var templates = await GetAvailableTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Id == templateId);
        
        if (template == null)
        {
            throw new ArgumentException($"Template '{templateId}' not found in catalog", nameof(templateId));
        }

        var isoPath = await DownloadAsync(template.DownloadUrl, progress);

        // Verify checksum if provided
        if (template.Checksum != null && !string.IsNullOrEmpty(template.Checksum.Value))
        {
            var isValid = await VerifyChecksumAsync(isoPath, template.Checksum);
            if (!isValid)
            {
                File.Delete(isoPath);
                throw new InvalidOperationException($"Checksum verification failed for {template.Name}");
            }
        }

        return isoPath;
    }

    public async Task<bool> VerifyChecksumAsync(string filePath, ISOChecksum checksum)
    {
        if (checksum.Algorithm.ToUpperInvariant() != "SHA256")
        {
            throw new NotSupportedException($"Checksum algorithm '{checksum.Algorithm}' not supported");
        }

        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        
        var hashBytes = await sha256.ComputeHashAsync(stream);
        var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        var expectedHash = checksum.Value.ToLowerInvariant();

        return computedHash == expectedHash;
    }

    public async Task<ISOCatalog?> GetCatalogAsync()
    {
        if (_catalog == null)
        {
            await LoadCatalogAsync();
        }
        return _catalog;
    }
}
