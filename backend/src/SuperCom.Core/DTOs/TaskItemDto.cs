using SuperCom.Core.Enums;

namespace SuperCom.Core.DTOs;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserTelephone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string UserFullName { get; set; } = string.Empty;
    public string UserTelephone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new();
}

public class UpdateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserTelephone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new();
}
