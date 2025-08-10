using Microsoft.EntityFrameworkCore;
using UserAuthAPI.Data.Entities;
using UserAuthAPI.Data.Interfaces;

namespace UserAuthAPI.Data.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetByIdForUserAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId, cancellationToken);
    }
}