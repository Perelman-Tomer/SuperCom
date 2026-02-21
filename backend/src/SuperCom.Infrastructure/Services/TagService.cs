using Microsoft.EntityFrameworkCore;
using SuperCom.Core.DTOs;
using SuperCom.Core.Entities;
using SuperCom.Core.Interfaces;
using SuperCom.Infrastructure.Data;

namespace SuperCom.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly SuperComDbContext _context;

    public TagService(SuperComDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TagDto?> GetByIdAsync(int id)
    {
        var tag = await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto)
    {
        var exists = await _context.Tags.AnyAsync(t => t.Name == dto.Name);
        if (exists)
            throw new InvalidOperationException($"A tag with the name '{dto.Name}' already exists.");

        var tag = new Tag { Name = dto.Name };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TagDto?> UpdateAsync(int id, UpdateTagDto dto)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return null;

        var duplicate = await _context.Tags.AnyAsync(t => t.Name == dto.Name && t.Id != id);
        if (duplicate)
            throw new InvalidOperationException($"A tag with the name '{dto.Name}' already exists.");

        tag.Name = dto.Name;
        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}
