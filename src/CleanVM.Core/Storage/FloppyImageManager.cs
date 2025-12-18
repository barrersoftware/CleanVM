using System.Diagnostics;

namespace CleanVM.Core.Storage;

/// <summary>
/// Manages floppy disk images - creates, formats, adds files
/// Used for older Windows installs that need driver floppies
/// </summary>
public class FloppyImageManager
{
    private readonly string _floppyStoragePath;

    public FloppyImageManager(string? storagePath = null)
    {
        _floppyStoragePath = storagePath ?? "/var/lib/cleanvm/floppies";
        
        if (!Directory.Exists(_floppyStoragePath))
            Directory.CreateDirectory(_floppyStoragePath);
    }

    /// <summary>
    /// Create a blank 1.44MB floppy image
    /// </summary>
    public async Task<string> CreateBlankFloppyAsync(string name, CancellationToken cancellationToken = default)
    {
        var floppyPath = Path.Combine(_floppyStoragePath, $"{name}.img");
        
        // Create 1.44MB floppy image using dd
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dd",
                Arguments = $"if=/dev/zero of={floppyPath} bs=1024 count=1440",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to create floppy image: {error}");
        }

        return floppyPath;
    }

    /// <summary>
    /// Create a floppy image from a directory of files
    /// </summary>
    public async Task<string> CreateFloppyFromDirectoryAsync(string name, string sourceDir, CancellationToken cancellationToken = default)
    {
        var floppyPath = await CreateBlankFloppyAsync(name, cancellationToken);

        // Format as FAT12 (floppy standard)
        await FormatFloppyAsync(floppyPath, cancellationToken);

        // Copy files to floppy
        await CopyFilesToFloppyAsync(floppyPath, sourceDir, cancellationToken);

        return floppyPath;
    }

    /// <summary>
    /// Format floppy image as FAT12
    /// </summary>
    private async Task FormatFloppyAsync(string floppyPath, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "mkfs.fat",
                Arguments = $"-F 12 {floppyPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to format floppy: {error}");
        }
    }

    /// <summary>
    /// Copy files from directory to floppy using mcopy (mtools)
    /// </summary>
    private async Task CopyFilesToFloppyAsync(string floppyPath, string sourceDir, CancellationToken cancellationToken)
    {
        // Use mcopy to copy files to FAT floppy image
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "mcopy",
                Arguments = $"-i {floppyPath} {sourceDir}/* ::",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to copy files to floppy: {error}");
        }
    }

    /// <summary>
    /// List all floppy images
    /// </summary>
    public IEnumerable<string> ListFloppyImages()
    {
        if (!Directory.Exists(_floppyStoragePath))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(_floppyStoragePath, "*.img");
    }

    /// <summary>
    /// Delete a floppy image
    /// </summary>
    public void DeleteFloppy(string name)
    {
        var floppyPath = Path.Combine(_floppyStoragePath, $"{name}.img");
        if (File.Exists(floppyPath))
            File.Delete(floppyPath);
    }

    /// <summary>
    /// Get floppy path by name
    /// </summary>
    public string GetFloppyPath(string name)
    {
        return Path.Combine(_floppyStoragePath, $"{name}.img");
    }
}
