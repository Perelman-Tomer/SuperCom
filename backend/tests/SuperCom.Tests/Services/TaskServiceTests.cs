using Microsoft.EntityFrameworkCore;
using SuperCom.Core.DTOs;
using SuperCom.Core.Entities;
using SuperCom.Core.Enums;
using SuperCom.Infrastructure.Data;
using SuperCom.Infrastructure.Services;

namespace SuperCom.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly SuperComDbContext _context;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<SuperComDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SuperComDbContext(options);
        _service = new TaskService(_context);

        SeedTags();
    }

    private void SeedTags()
    {
        _context.Tags.AddRange(
            new Tag { Id = 1, Name = "Urgent" },
            new Tag { Id = 2, Name = "Bug" },
            new Tag { Id = 3, Name = "Feature" }
        );
        _context.SaveChanges();
    }

    private CreateTaskItemDto CreateValidDto(string title = "Test Task") => new()
    {
        Title = title,
        Description = "Test description",
        DueDate = DateTime.UtcNow.AddDays(7),
        Priority = Priority.Medium,
        UserFullName = "John Doe",
        UserTelephone = "050-1234567",
        UserEmail = "john@example.com",
        TagIds = new List<int> { 1, 2 }
    };

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedTask()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test description", result.Description);
        Assert.Equal(Priority.Medium, result.Priority);
        Assert.Equal("John Doe", result.UserFullName);
        Assert.Equal("john@example.com", result.UserEmail);
        Assert.Equal(2, result.Tags.Count);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTagIds_IgnoresInvalidTags()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.TagIds = new List<int> { 1, 999 }; // 999 doesn't exist

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("Urgent", result.Tags[0].Name);
    }

    [Fact]
    public async Task CreateAsync_WithNoTags_ReturnsTaskWithEmptyTags()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.TagIds = new List<int>();

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        // Arrange
        await _service.CreateAsync(CreateValidDto("Task 1"));
        await _service.CreateAsync(CreateValidDto("Task 2"));
        await _service.CreateAsync(CreateValidDto("Task 3"));

        // Act
        var results = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        await _service.CreateAsync(CreateValidDto("First"));
        await _service.CreateAsync(CreateValidDto("Second"));
        await _service.CreateAsync(CreateValidDto("Third"));

        // Act
        var results = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal("Third", results[0].Title);
        Assert.Equal("Second", results[1].Title);
        Assert.Equal("First", results[2].Title);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTask()
    {
        // Arrange
        var created = await _service.CreateAsync(CreateValidDto());

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange
        var created = await _service.CreateAsync(CreateValidDto());
        var updateDto = new UpdateTaskItemDto
        {
            Title = "Updated Title",
            Description = "Updated description",
            DueDate = DateTime.UtcNow.AddDays(14),
            Priority = Priority.High,
            UserFullName = "Jane Doe",
            UserTelephone = "050-7654321",
            UserEmail = "jane@example.com",
            TagIds = new List<int> { 3 }
        };

        // Act
        var result = await _service.UpdateAsync(created.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal("Jane Doe", result.UserFullName);
        Assert.Single(result.Tags);
        Assert.Equal("Feature", result.Tags[0].Name);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTask_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateTaskItemDto
        {
            Title = "Updated",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = Priority.Low,
            UserFullName = "Test",
            UserTelephone = "050-1234567",
            UserEmail = "test@test.com",
            TagIds = new List<int>()
        };

        // Act
        var result = await _service.UpdateAsync(999, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ChangeTags_ReplacesExistingTags()
    {
        // Arrange - create with tags 1, 2
        var dto = CreateValidDto();
        dto.TagIds = new List<int> { 1, 2 };
        var created = await _service.CreateAsync(dto);

        // Act - update with tag 3 only
        var updateDto = new UpdateTaskItemDto
        {
            Title = "Test Task",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = Priority.Medium,
            UserFullName = "John Doe",
            UserTelephone = "050-1234567",
            UserEmail = "john@example.com",
            TagIds = new List<int> { 3 }
        };
        var result = await _service.UpdateAsync(created.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal("Feature", result.Tags[0].Name);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTask_ReturnsTrue()
    {
        // Arrange
        var created = await _service.CreateAsync(CreateValidDto());

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTask_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ReturnsOnlyOverdueTasks()
    {
        // Arrange
        var futureDto = CreateValidDto("Future Task");
        futureDto.DueDate = DateTime.UtcNow.AddDays(7);
        await _service.CreateAsync(futureDto);

        // Create an overdue task directly in the database
        var overdueTask = new TaskItem
        {
            Title = "Overdue Task",
            DueDate = DateTime.UtcNow.AddDays(-1),
            Priority = Priority.High,
            UserFullName = "Test User",
            UserTelephone = "050-0000000",
            UserEmail = "test@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        _context.Tasks.Add(overdueTask);
        await _context.SaveChangesAsync();

        // Act
        var results = (await _service.GetOverdueTasksAsync()).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Overdue Task", results[0].Title);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
