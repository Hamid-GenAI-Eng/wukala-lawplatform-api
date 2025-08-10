using Microsoft.AspNetCore.Http;
using UserAuthAPI.DTOs;

namespace UserAuthAPI.Services.Interfaces;

public interface IDocumentService
{
    Task<UploadDocumentResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentMetadataDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName, string FileType)?> DownloadAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default);
}