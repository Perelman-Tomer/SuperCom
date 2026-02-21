namespace SuperCom.Core.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
