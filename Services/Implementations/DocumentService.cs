using Microsoft.AspNetCore.Http;
using UserAuthAPI.Data.Entities;
using UserAuthAPI.Data.Interfaces;
using UserAuthAPI.DTOs;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Services.Implementations;

public class DocumentService : IDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".txt"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IDocumentRepository _documentRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IEncryptionService encryptionService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds limit of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException("Unsupported file type.");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, cancellationToken);
            fileBytes = ms.ToArray();
        }

        var encrypted = _encryptionService.Encrypt(fileBytes);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = Path.GetFileName(file.FileName),
            FileType = extension.TrimStart('.')
                                 .ToLowerInvariant(),
            UploadDate = DateTime.UtcNow,
            EncryptedContent = encrypted
        };

        var created = await _documentRepository.AddAsync(document, cancellationToken);

        _logger.LogInformation("Document {DocumentId} uploaded for user {UserId}", created.Id, userId);

        return new UploadDocumentResponse
        {
            DocumentId = created.Id,
            FileName = created.FileName,
            FileType = created.FileType,
            UploadDate = created.UploadDate
        };
    }

    public async Task<IReadOnlyList<DocumentMetadataDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByUserAsync(userId, cancellationToken);
        return documents
            .Select(d => new DocumentMetadataDto
            {
                Id = d.Id,
                FileName = d.FileName,
                FileType = d.FileType,
                UploadDate = d.UploadDate
            })
            .ToList();
    }

    public async Task<(byte[] Content, string FileName, string FileType)?> DownloadAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdForUserAsync(documentId, userId, cancellationToken);
        if (document == null)
            return null;

        var decrypted = _encryptionService.Decrypt(document.EncryptedContent);
        var fileType = document.FileType;
        var fileName = document.FileName;
        return (decrypted, fileName, fileType);
    }
}