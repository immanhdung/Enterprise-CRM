namespace EnterpriseCRM.Application.Common.Interfaces;

public interface IDocumentStorageService
{
    Task<string> UploadDocumentAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadDocumentAsync(string path, CancellationToken cancellationToken = default);
    Task<string> GetDocumentDownloadUrlAsync(string path, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(string path, CancellationToken cancellationToken = default);
}
