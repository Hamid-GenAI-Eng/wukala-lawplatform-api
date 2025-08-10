using System.ComponentModel.DataAnnotations;

namespace UserAuthAPI.DTOs;

public class DocumentMetadataDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

public class UploadDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

public class DownloadDocumentRequest
{
    [Required]
    public Guid DocumentId { get; set; }
}