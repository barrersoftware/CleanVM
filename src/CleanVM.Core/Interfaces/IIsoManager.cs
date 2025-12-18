using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface IIsoManager
{
    // ISO Management
    Task<Iso> ImportIsoAsync(IsoImportRequest request, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<Iso> GetIsoAsync(string isoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Iso>> ListIsosAsync(CancellationToken cancellationToken = default);
    Task DeleteIsoAsync(string isoId, CancellationToken cancellationToken = default);
    
    // ISO Detection
    Task<IsoMetadata> DetectIsoAsync(string isoPath, CancellationToken cancellationToken = default);
    Task<IsoMetadata> AnalyzeIsoAsync(Stream isoStream, CancellationToken cancellationToken = default);
    
    // ISO Download
    Task<Iso> DownloadIsoAsync(string url, string? name = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<IsoTemplate>> GetAvailableTemplatesAsync(CancellationToken cancellationToken = default);
    Task<Iso> DownloadTemplateAsync(string templateId, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    
    // ISO Operations
    Task AttachIsoToVmAsync(string vmId, string isoId, CancellationToken cancellationToken = default);
    Task DetachIsoFromVmAsync(string vmId, CancellationToken cancellationToken = default);
    Task<Iso?> GetVmAttachedIsoAsync(string vmId, CancellationToken cancellationToken = default);
    
    // VM Creation Helpers
    Task<VmRecommendedSettings> GetRecommendedSettingsAsync(string isoId, CancellationToken cancellationToken = default);
}
