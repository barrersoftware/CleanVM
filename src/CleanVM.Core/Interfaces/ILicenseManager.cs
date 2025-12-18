namespace CleanVM.Core.Interfaces;

public interface ILicenseManager
{
    Task<LicenseInfo> GetLicenseInfoAsync(CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
}

public record LicenseInfo(
    LicenseType Type,
    bool IsValid,
    DateTime? ExpirationDate,
    string[] EnabledFeatures
);

public enum LicenseType
{
    Community,
    Enterprise
}
