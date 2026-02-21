using Microsoft.EntityFrameworkCore;
using SuperCom.Core.DTOs;
using SuperCom.Core.Entities;
using SuperCom.Infrastructure.Data;
using SuperCom.Infrastructure.Services;

namespace SuperCom.Tests.Services;

public class TagServiceTests : IDisposable
{
    private readonly SuperComDbContext _context;
    private readonly TagService _service;

    public TagServiceTests()
    {
        var options = new DbContextOptionsBuilder<SuperComDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SuperComDbContext(options);
        _service = new TagService(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTags_OrderedByName()
    {
        // Arrange
        _context.Tags.AddRange(
            new Tag { Id = 100, Name = "Zeta" },
            new Tag { Id = 101, Name = "Alpha" },
            new Tag { Id = 102, Name = "Middle" }
        );
        await _context.SaveChangesAsync();

        // Act
        var results = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.True(results.Count >= 3);
        // Verify order - Alpha should come before Middle, Middle before Zeta
        var alpha = results.FindIndex(t => t.Name == "Alpha");
        var middle = results.FindIndex(t => t.Name == "Middle");
        var zeta = results.FindIndex(t => t.Name == "Zeta");
        Assert.True(alpha < middle);
        Assert.True(middle < zeta);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTag_ReturnsTag()
    {
        // Arrange
        _context.Tags.Add(new Tag { Id = 200, Name = "TestTag" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(200);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Id);
        Assert.Equal("TestTag", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTag_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedTag()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "NewTag" };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("NewTag", result.Name);
    }

    [Fact]
    public async Task CreateAsync_VerifyPersistedInDatabase()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "Persisted" };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        var fromDb = await _context.Tags.FindAsync(result.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Persisted", fromDb.Name);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTag_ReturnsTrue()
    {
        // Arrange
        _context.Tags.Add(new Tag { Id = 300, Name = "ToDelete" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(300);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Tags.FindAsync(300));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTag_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTag_ReturnsUpdatedTag()
    {
        // Arrange
        _context.Tags.Add(new Tag { Id = 400, Name = "OldName" });
        await _context.SaveChangesAsync();
        var dto = new UpdateTagDto { Name = "NewName" };

        // Act
        var result = await _service.UpdateAsync(400, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.Id);
        Assert.Equal("NewName", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTag_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateTagDto { Name = "Whatever" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        _context.Tags.AddRange(
            new Tag { Id = 500, Name = "First" },
            new Tag { Id = 501, Name = "Second" }
        );
        await _context.SaveChangesAsync();
        var dto = new UpdateTagDto { Name = "Second" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(500, dto));
    }

    [Fact]
    public async Task UpdateAsync_SameNameAsSelf_Succeeds()
    {
        // Arrange
        _context.Tags.Add(new Tag { Id = 600, Name = "SameName" });
        await _context.SaveChangesAsync();
        var dto = new UpdateTagDto { Name = "SameName" };

        // Act
        var result = await _service.UpdateAsync(600, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SameName", result.Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
