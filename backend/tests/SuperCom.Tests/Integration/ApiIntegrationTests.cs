using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuperCom.Core.DTOs;
using SuperCom.Core.Enums;
using SuperCom.Infrastructure.Data;

namespace SuperCom.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL Entity Framework Core related descriptors (ServiceType + ImplementationType)
            // to prevent dual-provider (SqlServer + InMemory) conflict
            var toRemove = services
                .Where(d =>
                {
                    var stName = d.ServiceType.FullName ?? string.Empty;
                    var itName = d.ImplementationType?.FullName ?? string.Empty;
                    return stName.Contains("EntityFrameworkCore")
                        || stName.Contains("SuperComDbContext")
                        || itName.Contains("EntityFrameworkCore")
                        || itName.Contains("SuperComDbContext");
                })
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing (name must be captured OUTSIDE the lambda
            // so each request scope within the same factory shares the same database)
            var dbName = "TestDb_" + Guid.NewGuid();
            services.AddDbContext<SuperComDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}

public class TasksApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TasksApiTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    private CreateTaskItemDto CreateValidDto(string title = "Integration Test Task") => new()
    {
        Title = title,
        Description = "Integration test description",
        DueDate = DateTime.UtcNow.AddDays(7),
        Priority = Priority.Medium,
        UserFullName = "Test User",
        UserTelephone = "050-1234567",
        UserEmail = "test@example.com",
        TagIds = new List<int>()
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>(JsonOptions);
        Assert.NotNull(tasks);
    }

    [Fact]
    public async Task Create_ValidTask_ReturnsCreated()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Integration Test Task", created.Title);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Create_InvalidTask_ReturnsBadRequest()
    {
        // Arrange - empty title
        var dto = CreateValidDto();
        dto.Title = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingTask_ReturnsOk()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", CreateValidDto());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{created!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);
        Assert.NotNull(task);
        Assert.Equal(created.Id, task.Id);
    }

    [Fact]
    public async Task GetById_NonExistingTask_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingTask_ReturnsOk()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", CreateValidDto());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);

        var updateDto = new UpdateTaskItemDto
        {
            Title = "Updated Title",
            Description = "Updated",
            DueDate = DateTime.UtcNow.AddDays(14),
            Priority = Priority.High,
            UserFullName = "Updated User",
            UserTelephone = "050-9999999",
            UserEmail = "updated@example.com",
            TagIds = new List<int>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{created!.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);
        Assert.Equal("Updated Title", updated!.Title);
        Assert.Equal(Priority.High, updated.Priority);
    }

    [Fact]
    public async Task Delete_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", CreateValidDto());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>(JsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingTask_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}

public class TagsApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TagsApiTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tags");

        // Assert
        response.EnsureSuccessStatusCode();
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>(JsonOptions);
        Assert.NotNull(tags);
    }

    [Fact]
    public async Task Create_ValidTag_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "IntegrationTag" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TagDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("IntegrationTag", created.Name);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTag_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tags", new CreateTagDto { Name = "ToDelete" });
        var created = await createResponse.Content.ReadFromJsonAsync<TagDto>(JsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/tags/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingTag_ReturnsOk()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tags", new CreateTagDto { Name = "BeforeUpdate" });
        var created = await createResponse.Content.ReadFromJsonAsync<TagDto>(JsonOptions);
        var updateDto = new UpdateTagDto { Name = "AfterUpdate" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tags/{created!.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<TagDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("AfterUpdate", updated.Name);
    }

    [Fact]
    public async Task Update_NonExistingTag_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateTagDto { Name = "Doesn't matter" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tags/99999", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/tags", new CreateTagDto { Name = "ValidTag" });
        var created = await createResponse.Content.ReadFromJsonAsync<TagDto>(JsonOptions);
        var updateDto = new UpdateTagDto { Name = "" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tags/{created!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
