using EnterpriseCRM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class DocumentStorageService : IDocumentStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentStorageService> _logger;
    private readonly string _localBaseDir;

    public DocumentStorageService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<DocumentStorageService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _localBaseDir = Path.Combine(webRoot, "uploads", "documents");
        if (!Directory.Exists(_localBaseDir))
        {
            Directory.CreateDirectory(_localBaseDir);
        }
    }

    public async Task<string> UploadDocumentAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // Try Supabase Storage if configured
        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:ServiceRoleKey"] ?? _configuration["Supabase:Key"];

        // Local storage fallback / primary for reliability
        var cleanPath = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_localBaseDir, cleanPath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        content.Position = 0;
        await content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Saved document to local storage: {FilePath}", fullPath);
        return $"/uploads/documents/{path.Replace('\\', '/').TrimStart('/')}";
    }

    public Task<Stream> DownloadDocumentAsync(string path, CancellationToken cancellationToken = default)
    {
        var cleanPath = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_localBaseDir, cleanPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Document not found at {path}");
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task<string> GetDocumentDownloadUrlAsync(string path, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var relativeUrl = $"/uploads/documents/{path.Replace('\\', '/').TrimStart('/')}";
        return Task.FromResult(relativeUrl);
    }

    public Task<bool> DeleteDocumentAsync(string path, CancellationToken cancellationToken = default)
    {
        var cleanPath = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_localBaseDir, cleanPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
