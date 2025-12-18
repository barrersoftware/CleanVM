using CleanVM.Core.Interfaces;

namespace CleanVM.Core.ISO;

/// <summary>
/// Manages a library of ISO files - scans directories, caches metadata, provides search
/// </summary>
public class ISOLibraryManager : IISOLibraryManager
{
    private readonly IISODetector _detector;
    private readonly List<string> _libraryPaths = new();
    private readonly Dictionary<string, ISOInfo> _cache = new();
    private readonly string _cacheFile;

    public ISOLibraryManager(IISODetector detector, string? cacheFilePath = null)
    {
        _detector = detector;
        _cacheFile = cacheFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cleanvm",
            "iso-library-cache.json"
        );
    }

    public void AddLibraryPath(string path)
    {
        if (Directory.Exists(path) && !_libraryPaths.Contains(path))
        {
            _libraryPaths.Add(path);
        }
    }

    public async Task<IEnumerable<ISOInfo>> ScanLibraryAsync(CancellationToken cancellationToken = default)
    {
        var isos = new List<ISOInfo>();

        foreach (var libraryPath in _libraryPaths)
        {
            var isoFiles = Directory.GetFiles(libraryPath, "*.iso", SearchOption.TopDirectoryOnly);
            
            foreach (var isoPath in isoFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Check if we have it in cache and file hasn't changed
                if (_cache.TryGetValue(isoPath, out var cachedInfo))
                {
                    var fileInfo = new FileInfo(isoPath);
                    if (fileInfo.Length == cachedInfo.SizeBytes)
                    {
                        isos.Add(cachedInfo);
                        continue;
                    }
                }

                // Not in cache or file changed - detect it
                var info = await _detector.DetectAsync(isoPath, cancellationToken);
                if (info != null)
                {
                    _cache[isoPath] = info;
                    isos.Add(info);
                }
            }
        }

        return isos;
    }

    public async Task<ISOInfo?> GetISOInfoAsync(string path, CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(path, out var cachedInfo))
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length == cachedInfo.SizeBytes)
                return cachedInfo;
        }

        // Not in cache - detect it
        var info = await _detector.DetectAsync(path, cancellationToken);
        if (info != null)
        {
            _cache[path] = info;
        }

        return info;
    }

    public async Task<IEnumerable<ISOInfo>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var allISOs = await ScanLibraryAsync(cancellationToken);
        var lowerQuery = query.ToLowerInvariant();

        return allISOs.Where(iso =>
            iso.OS.Name.ToLowerInvariant().Contains(lowerQuery) ||
            iso.OS.Version.ToLowerInvariant().Contains(lowerQuery) ||
            Path.GetFileName(iso.Path).ToLowerInvariant().Contains(lowerQuery)
        );
    }

    public async Task<IEnumerable<ISOInfo>> GetByOSTypeAsync(OSType osType, CancellationToken cancellationToken = default)
    {
        var allISOs = await ScanLibraryAsync(cancellationToken);
        return allISOs.Where(iso => iso.OS.Type == osType);
    }

    public async Task SaveCacheAsync(CancellationToken cancellationToken = default)
    {
        var cacheDir = Path.GetDirectoryName(_cacheFile);
        if (!string.IsNullOrEmpty(cacheDir) && !Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(_cache, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await File.WriteAllTextAsync(_cacheFile, json, cancellationToken);
    }

    public async Task LoadCacheAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_cacheFile))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(_cacheFile, cancellationToken);
            var cache = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ISOInfo>>(json);
            
            if (cache != null)
            {
                _cache.Clear();
                foreach (var kvp in cache)
                {
                    // Only load if file still exists
                    if (File.Exists(kvp.Key))
                    {
                        _cache[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch
        {
            // Cache file corrupted or incompatible - ignore
        }
    }

    public IEnumerable<string> GetLibraryPaths() => _libraryPaths;

    public int GetCachedCount() => _cache.Count;
}
