using IllustratorService.Models;
using IllustratorService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IllustratorService.Controllers;

[ApiController]
[Route("api/illustrators")]
public class IllustratorsController : ControllerBase
{
    private readonly IIllustratorService _illustratorService;
    private readonly ILogger<IllustratorsController> _logger;

    public IllustratorsController(
        IIllustratorService illustratorService,
        ILogger<IllustratorsController> logger)
    {
        _illustratorService = illustratorService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<IllustratorResponse>> Create([FromBody] CreateIllustratorRequest request)
    {
        var userId = HttpContext.Items["UserId"] as string;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        try
        {
            var illustrator = await _illustratorService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = illustrator.Id }, illustrator);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating illustrator profile for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while creating the profile" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? specialty = null,
        [FromQuery] bool? isAvailableForWork = null)
    {
        try
        {
            var (illustrators, totalCount) = await _illustratorService.GetAllAsync(
                page, pageSize, specialty, isAvailableForWork);

            return Ok(new
            {
                illustrators,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving illustrators list");
            return StatusCode(500, new { error = "An error occurred while retrieving illustrators" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IllustratorResponse>> GetById(int id)
    {
        try
        {
            var illustrator = await _illustratorService.GetByIdAsync(id);

            if (illustrator == null)
            {
                return NotFound(new { error = $"Illustrator with ID {id} not found" });
            }

            return Ok(illustrator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving illustrator {IllustratorId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the profile" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IllustratorResponse>> GetByUserId(string userId)
    {
        try
        {
            var illustrator = await _illustratorService.GetByUserIdAsync(userId);

            if (illustrator == null)
            {
                return NotFound(new { error = $"Illustrator profile not found for user {userId}" });
            }

            return Ok(illustrator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving illustrator for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving the profile" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<IllustratorResponse>> Update(
        int id,
        [FromBody] UpdateIllustratorRequest request)
    {
        var userId = HttpContext.Items["UserId"] as string;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        try
        {
            var illustrator = await _illustratorService.UpdateAsync(id, request, userId);
            return Ok(illustrator);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating illustrator {IllustratorId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the profile" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Items["UserId"] as string;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        try
        {
            await _illustratorService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting illustrator {IllustratorId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the profile" });
        }
    }
}
