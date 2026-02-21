using SuperCom.Core.DTOs;

namespace SuperCom.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskItemDto>> GetAllAsync();
    Task<TaskItemDto?> GetByIdAsync(int id);
    Task<TaskItemDto> CreateAsync(CreateTaskItemDto dto);
    Task<TaskItemDto?> UpdateAsync(int id, UpdateTaskItemDto dto);
    Task<bool> DeleteAsync(int id);
    Task<TaskItemDto?> ToggleCompletionAsync(int id);
    Task<IEnumerable<TaskItemDto>> GetOverdueTasksAsync();
}
