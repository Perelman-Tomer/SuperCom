using Mapster;
using Microsoft.EntityFrameworkCore;
using SuperCom.Core.DTOs;
using SuperCom.Core.Entities;
using SuperCom.Core.Interfaces;
using SuperCom.Infrastructure.Data;

namespace SuperCom.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly SuperComDbContext _context;

    public TaskService(SuperComDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllAsync()
    {
        var tasks = await _context.Tasks
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskItemDto?> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskItemDto> CreateAsync(CreateTaskItemDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            UserFullName = dto.UserFullName,
            UserTelephone = dto.UserTelephone,
            UserEmail = dto.UserEmail,
            CreatedAt = DateTime.UtcNow
        };

        // Add tag associations
        if (dto.TagIds.Any())
        {
            var validTagIds = await _context.Tags
                .Where(t => dto.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var tagId in validTagIds)
            {
                task.TaskTags.Add(new TaskTag { TagId = tagId });
            }
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(task.Id))!;
    }

    public async Task<TaskItemDto?> UpdateAsync(int id, UpdateTaskItemDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.Priority = dto.Priority;
        task.UserFullName = dto.UserFullName;
        task.UserTelephone = dto.UserTelephone;
        task.UserEmail = dto.UserEmail;
        task.UpdatedAt = DateTime.UtcNow;

        // Update tag associations - remove existing, add new
        task.TaskTags.Clear();

        if (dto.TagIds.Any())
        {
            var validTagIds = await _context.Tags
                .Where(t => dto.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var tagId in validTagIds)
            {
                task.TaskTags.Add(new TaskTag { TaskItemId = id, TagId = tagId });
            }
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TaskItemDto?> ToggleCompletionAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        task.IsCompleted = !task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync()
    {
        var tasks = await _context.Tasks
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted)
            .AsNoTracking()
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    private static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = task.Priority,
            UserFullName = task.UserFullName,
            UserTelephone = task.UserTelephone,
            UserEmail = task.UserEmail,
            IsCompleted = task.IsCompleted,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Tags = task.TaskTags.Select(tt => new TagDto
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name
            }).ToList()
        };
    }
}
