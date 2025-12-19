using CleanVM.Core.ISO;

namespace CleanVM.Core.Interfaces;

public interface IISODownloader
{
    Task<string> DownloadAsync(string url, IProgress<DownloadProgress>? progress = null);
    Task<IEnumerable<ISOTemplate>> GetAvailableTemplatesAsync();
}

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
}
