using UserAuthAPI.Data.Entities;

namespace UserAuthAPI.Data.Interfaces;

public interface IDocumentRepository
{
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdForUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default);
}