using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SuperCom.Core.DTOs;
using SuperCom.Core.Interfaces;

namespace SuperCom.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly IValidator<CreateTagDto> _createValidator;
    private readonly IValidator<UpdateTagDto> _updateValidator;

    public TagsController(
        ITagService tagService,
        IValidator<CreateTagDto> createValidator,
        IValidator<UpdateTagDto> updateValidator)
    {
        _tagService = tagService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get a specific tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetById(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag == null)
            return NotFound(new { message = $"Tag with ID {id} not found." });

        return Ok(tag);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        var tag = await _tagService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _tagService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Tag with ID {id} not found." });

        return NoContent();
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> Update(int id, [FromBody] UpdateTagDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        var tag = await _tagService.UpdateAsync(id, dto);
        if (tag == null)
            return NotFound(new { message = $"Tag with ID {id} not found." });

        return Ok(tag);
    }
}
