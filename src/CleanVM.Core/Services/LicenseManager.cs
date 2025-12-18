using CleanVM.Core.Interfaces;

namespace CleanVM.Core.Services;

public class LicenseManager : ILicenseManager
{
    private const string LicenseFilePath = "/etc/cleanvm/license.key";
    private LicenseInfo? _cachedLicense;

    public async Task<LicenseInfo> GetLicenseInfoAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedLicense != null)
            return _cachedLicense;

        if (!File.Exists(LicenseFilePath))
        {
            _cachedLicense = new LicenseInfo(
                LicenseType.Community,
                true,
                null,
                Array.Empty<string>()
            );
            return _cachedLicense;
        }

        var licenseKey = await File.ReadAllTextAsync(LicenseFilePath, cancellationToken);
        var isValid = await ValidateLicenseKeyAsync(licenseKey, cancellationToken);

        _cachedLicense = new LicenseInfo(
            LicenseType.Enterprise,
            isValid,
            null,
            new[] { "API", "CLI", "AdvancedUserManagement", "Deployment" }
        );

        return _cachedLicense;
    }

    public async Task<bool> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        if (!await ValidateLicenseKeyAsync(licenseKey, cancellationToken))
            return false;

        Directory.CreateDirectory(Path.GetDirectoryName(LicenseFilePath)!);
        await File.WriteAllTextAsync(LicenseFilePath, licenseKey, cancellationToken);
        
        _cachedLicense = null; // Clear cache
        return true;
    }

    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var license = await GetLicenseInfoAsync(cancellationToken);
        return license.IsValid;
    }

    private Task<bool> ValidateLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken)
    {
        // TODO: Implement proper license validation
        // For now, just check if it's not empty
        return Task.FromResult(!string.IsNullOrWhiteSpace(licenseKey));
    }
}
