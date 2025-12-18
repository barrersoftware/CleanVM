namespace CleanVM.Core.Interfaces;

public interface IISODownloader
{
    Task<string> DownloadAsync(string url, IProgress<DownloadProgress>? progress = null);
    Task<IEnumerable<ISOTemplate>> GetAvailableTemplatesAsync();
}

public class ISOTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
}
