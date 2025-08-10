using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserAuthAPI.DTOs;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadDocumentResponse>>> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var response = await _documentService.UploadAsync(userId, file, cancellationToken);
            return Ok(ApiResponse<UploadDocumentResponse>.SuccessResponse(response, "File uploaded successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request");
            return BadRequest(ApiResponse<UploadDocumentResponse>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, ApiResponse<UploadDocumentResponse>.ErrorResponse("An error occurred while uploading the document"));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentMetadataDto>>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var documents = await _documentService.GetAllForUserAsync(userId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<DocumentMetadataDto>>.SuccessResponse(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents");
            return StatusCode(500, ApiResponse<IReadOnlyList<DocumentMetadataDto>>.ErrorResponse("An error occurred while fetching documents"));
        }
    }

    [HttpGet("download/{documentId}")]
    public async Task<IActionResult> Download([FromRoute] Guid documentId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var result = await _documentService.DownloadAsync(userId, documentId, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Document not found"));
            }

            var (content, fileName, fileType) = result.Value;
            var mimeType = GetMimeType(fileType);
            return File(content, mimeType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while downloading the document"));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }
        return userId;
    }

    private static string GetMimeType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "jpg" => "image/jpeg",
            "jpeg" => "image/jpeg",
            "png" => "image/png",
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}