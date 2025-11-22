using Microsoft.AspNetCore.Mvc;
using ImageService.Attributes;
using ImageService.Models.DTOs;
using ImageService.Models.Responses;
using ImageService.Services;

namespace ImageService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IS3Service _s3Service;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IImageService imageService,
        IS3Service s3Service,
        ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _s3Service = s3Service;
        _logger = logger;
    }
    // PROTECTED: Direct file upload
    [HttpPost("direct-upload")]
    [RequireAuthentication]
    public async Task<ActionResult<ImageResponse>> DirectUpload(
        IFormFile file,
        [FromForm] string title,
        [FromForm] string? description = null,
        [FromForm] string? tags = null)
    {
        var userIdStr = HttpContext.Items["UserId"]?.ToString();

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        try
        {
            // Upload full image
            using var imageStream = file.OpenReadStream();
            var imageUrl = await _s3Service.UploadImageAsync(
                imageStream,
                file.FileName,
                file.ContentType);

            // Create thumbnail (reuse same image for now)
            imageStream.Position = 0;
            var thumbnailUrl = await _s3Service.UploadThumbnailAsync(
                imageStream,
                file.FileName);

            // Parse tags
            var tagList = string.IsNullOrEmpty(tags)
                ? new List<string>()
                : tags.Split(',').Select(t => t.Trim()).ToList();

            // Create image metadata
            var createRequest = new CreateImageRequest
            {
                Title = title,
                Description = description ?? string.Empty,
                ImageUrl = imageUrl,
                ThumbnailUrl = thumbnailUrl,
                Tags = tagList,
                IsPublished = true
            };

            var image = await _imageService.CreateImageAsync(userId, createRequest);
            return CreatedAtAction(nameof(GetById), new { id = image.Id }, image);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { error = "Failed to upload image" });
        }
    }

    // PUBLIC: Get all images (main page)
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ImageResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tags = null,
        [FromQuery] string sortBy = "recent")
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _imageService.GetAllImagesAsync(page, pageSize, tags, sortBy);
        return Ok(result);
    }

    // PUBLIC: Get single image
    [HttpGet("{id}")]
    public async Task<ActionResult<ImageResponse>> GetById(Guid id)
    {
        var image = await _imageService.GetImageByIdAsync(id);

        if (image == null)
            return NotFound(new { error = "Image not found" });

        // Increment view count
        await _imageService.IncrementViewCountAsync(id);

        return Ok(image);
    }

    // PUBLIC: Get images by illustrator
    [HttpGet("illustrator/{illustratorId}")]
    public async Task<ActionResult<PagedResponse<ImageResponse>>> GetByIllustrator(
        Guid illustratorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _imageService.GetImagesByIllustratorAsync(illustratorId, page, pageSize);
        return Ok(result);
    }

    // PROTECTED: Generate presigned upload URL
    [HttpPost("upload-url")]
    [RequireAuthentication]
    public async Task<ActionResult<UploadUrlResponse>> GetUploadUrl(
        [FromQuery] string fileName,
        [FromQuery] string contentType = "image/jpeg")
    {
        try
        {
            var uploadResponse = await _s3Service.GeneratePresignedUploadUrlAsync(fileName, contentType);
            return Ok(uploadResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload URL");
            return StatusCode(500, new { error = "Failed to generate upload URL" });
        }
    }

    // PROTECTED: Create image metadata after S3 upload
    [HttpPost]
    [RequireAuthentication]
    public async Task<ActionResult<ImageResponse>> Create([FromBody] CreateImageRequest request)
    {
        var userIdStr = HttpContext.Items["UserId"]?.ToString();

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        try
        {
            var image = await _imageService.CreateImageAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = image.Id }, image);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating image");
            return StatusCode(500, new { error = "Failed to create image" });
        }
    }

    // PROTECTED: Update image
    [HttpPut("{id}")]
    [RequireAuthentication]
    public async Task<ActionResult<ImageResponse>> Update(
        Guid id,
        [FromBody] UpdateImageRequest request)
    {
        var userIdStr = HttpContext.Items["UserId"]?.ToString();

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var image = await _imageService.UpdateImageAsync(id, userId, request);

        if (image == null)
            return NotFound(new { error = "Image not found or unauthorized" });

        return Ok(image);
    }

    // PROTECTED: Delete image
    [HttpDelete("{id}")]
    [RequireAuthentication]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userIdStr = HttpContext.Items["UserId"]?.ToString();

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var image = await _imageService.GetImageByIdAsync(id);
        if (image == null)
            return NotFound();

        // Delete from S3
        await _s3Service.DeleteImageAsync(image.ImageUrl);
        await _s3Service.DeleteImageAsync(image.ThumbnailUrl);

        // Delete from database
        var success = await _imageService.DeleteImageAsync(id, userId);

        if (!success)
            return NotFound(new { error = "Image not found or unauthorized" });

        return NoContent();
    }
}