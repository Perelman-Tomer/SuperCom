using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SuperCom.Core.DTOs;
using SuperCom.Core.Interfaces;

namespace SuperCom.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidator<CreateTaskItemDto> _createValidator;
    private readonly IValidator<UpdateTaskItemDto> _updateValidator;

    public TasksController(
        ITaskService taskService,
        IValidator<CreateTaskItemDto> createValidator,
        IValidator<UpdateTaskItemDto> updateValidator)
    {
        _taskService = taskService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get all tasks with their tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { message = $"Task with ID {id} not found." });

        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskItemDto dto)
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

        var task = await _taskService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItemDto>> Update(int id, [FromBody] UpdateTaskItemDto dto)
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

        var task = await _taskService.UpdateAsync(id, dto);
        if (task == null)
            return NotFound(new { message = $"Task with ID {id} not found." });

        return Ok(task);
    }

    /// <summary>
    /// Toggle task completion status
    /// </summary>
    [HttpPatch("{id}/toggle-completion")]
    public async Task<ActionResult<TaskItemDto>> ToggleCompletion(int id)
    {
        var task = await _taskService.ToggleCompletionAsync(id);
        if (task == null)
            return NotFound(new { message = $"Task with ID {id} not found." });

        return Ok(task);
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _taskService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Task with ID {id} not found." });

        return NoContent();
    }
}
