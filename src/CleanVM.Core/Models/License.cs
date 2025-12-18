namespace CleanVM.Core.Models;

public class LicenseInfo
{
    public required string LicenseKey { get; init; }
    public LicenseType Type { get; set; }
    public required string LicensedTo { get; set; }
    public DateTime IssuedAt { get; init; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public List<string> EnabledFeatures { get; set; } = new();
}

public enum LicenseType
{
    Community,
    Enterprise
}

public class LicenseStatus
{
    public bool IsLicensed { get; set; }
    public LicenseType Type { get; set; }
    public string? LicensedTo { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DaysUntilExpiry { get; set; }
    public List<string> AvailableFeatures { get; set; } = new();
}
