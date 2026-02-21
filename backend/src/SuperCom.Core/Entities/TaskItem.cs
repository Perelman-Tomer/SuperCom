using SuperCom.Core.Enums;

namespace SuperCom.Core.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;

    // User Details
    public string UserFullName { get; set; } = string.Empty;
    public string UserTelephone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public bool ReminderSent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Concurrency token for safe concurrent updates (e.g. ReminderService)
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation - many-to-many via join entity
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
