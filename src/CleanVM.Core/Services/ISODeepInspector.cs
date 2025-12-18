using System.Diagnostics;
using System.Text;

namespace CleanVM.Core.Services;

/// <summary>
/// Deep ISO inspection - reads files inside ISOs to determine exact OS version
/// Uses isoinfo from cdrtools package (no mounting required!)
/// </summary>
public class ISODeepInspector
{
    /// <summary>
    /// List files in ISO without mounting
    /// </summary>
    public async Task<List<string>> ListFilesAsync(string isoPath, CancellationToken cancellationToken = default)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "isoinfo",
                Arguments = $"-i \"{isoPath}\" -f",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
            return new List<string>();

        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                     .Select(f => f.Trim())
                     .ToList();
    }

    /// <summary>
    /// Extract and read a specific file from ISO
    /// </summary>
    public async Task<string?> ReadFileAsync(string isoPath, string filePath, CancellationToken cancellationToken = default)
    {
        // isoinfo can extract files using -x flag
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "isoinfo",
                Arguments = $"-i \"{isoPath}\" -x \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        
        try
        {
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                return output;
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Check if specific file exists in ISO
    /// </summary>
    public async Task<bool> FileExistsAsync(string isoPath, string filePath, CancellationToken cancellationToken = default)
    {
        var files = await ListFilesAsync(isoPath, cancellationToken);
        return files.Any(f => f.ToLowerInvariant().Contains(filePath.ToLowerInvariant()));
    }

    /// <summary>
    /// Detect Windows version by inspecting sources folder
    /// </summary>
    public async Task<string?> DetectWindowsVersionAsync(string isoPath, CancellationToken cancellationToken = default)
    {
        var files = await ListFilesAsync(isoPath, cancellationToken);
        
        // Check for Windows identifier files
        var hasInstallWim = files.Any(f => f.ToLowerInvariant().Contains("/sources/install.wim"));
        var hasInstallEsd = files.Any(f => f.ToLowerInvariant().Contains("/sources/install.esd"));
        var hasIdwbinfo = files.Any(f => f.ToLowerInvariant().Contains("/sources/idwbinfo.txt"));
        
        if (!hasInstallWim && !hasInstallEsd)
            return null; // Not a Windows installer
        
        // Try to read idwbinfo.txt for build info
        if (hasIdwbinfo)
        {
            var idwbInfo = await ReadFileAsync(isoPath, "/SOURCES/IDWBINFO.TXT", cancellationToken);
            if (!string.IsNullOrEmpty(idwbInfo))
            {
                return ParseWindowsBuildInfo(idwbInfo);
            }
        }

        // Check for specific files that indicate version
        if (files.Any(f => f.ToLowerInvariant().Contains("win11")))
            return "11";
        if (files.Any(f => f.ToLowerInvariant().Contains("win10")))
            return "10";
        
        return "Unknown";
    }

    /// <summary>
    /// Detect Linux distro by reading distro-specific files
    /// </summary>
    public async Task<(string? Name, string? Version)?> DetectLinuxDistroAsync(string isoPath, CancellationToken cancellationToken = default)
    {
        var files = await ListFilesAsync(isoPath, cancellationToken);
        
        // Ubuntu: .disk/info
        if (files.Any(f => f.Contains(".disk/info")))
        {
            var info = await ReadFileAsync(isoPath, "/.disk/info", cancellationToken);
            if (!string.IsNullOrEmpty(info))
            {
                return ParseUbuntuInfo(info);
            }
        }
        
        // Fedora: .discinfo
        if (files.Any(f => f.Contains(".discinfo")))
        {
            var info = await ReadFileAsync(isoPath, "/.discinfo", cancellationToken);
            if (!string.IsNullOrEmpty(info))
            {
                return ("Fedora", ParseVersion(info));
            }
        }
        
        // Debian: .disk/info or README
        if (files.Any(f => f.ToLowerInvariant().Contains("debian")))
        {
            return ("Debian", null);
        }
        
        return null;
    }

    private string ParseWindowsBuildInfo(string idwbInfo)
    {
        // Parse build info from idwbinfo.txt
        // Format varies but usually contains build number and edition
        var lines = idwbInfo.Split('\n');
        
        foreach (var line in lines)
        {
            if (line.Contains("26100")) return "11/Server 2025";
            if (line.Contains("22621")) return "11";
            if (line.Contains("22000")) return "11";
            if (line.Contains("19044")) return "10";
            if (line.Contains("19043")) return "10";
            if (line.Contains("20348")) return "Server 2022";
        }
        
        return "Unknown";
    }

    private (string Name, string? Version) ParseUbuntuInfo(string info)
    {
        // Ubuntu info format: "Ubuntu 22.04.3 LTS \"Jammy Jellyfish\" - Release amd64"
        var match = System.Text.RegularExpressions.Regex.Match(info, @"Ubuntu\s+([\d.]+)");
        if (match.Success)
        {
            return ("Ubuntu", match.Groups[1].Value);
        }
        
        return ("Ubuntu", null);
    }

    private string? ParseVersion(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"[\d]+\.[\d]+");
        return match.Success ? match.Value : null;
    }
}
