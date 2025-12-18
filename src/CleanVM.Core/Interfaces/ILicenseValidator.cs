using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface ILicenseValidator
{
    // License Validation
    Task<LicenseInfo> ValidateLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);
    Task<bool> IsEnterpriseUnlockedAsync(CancellationToken cancellationToken = default);
    Task<LicenseInfo?> GetCurrentLicenseAsync(CancellationToken cancellationToken = default);
    
    // License Management
    Task ActivateLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);
    Task DeactivateLicenseAsync(CancellationToken cancellationToken = default);
    
    // Feature Checks
    Task<bool> IsFeatureAvailableAsync(string featureName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAvailableFeaturesAsync(CancellationToken cancellationToken = default);
    
    // License Info
    Task<LicenseStatus> GetLicenseStatusAsync(CancellationToken cancellationToken = default);
}
