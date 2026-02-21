using SuperCom.Core.DTOs;

namespace SuperCom.Core.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(int id);
    Task<TagDto> CreateAsync(CreateTagDto dto);
    Task<TagDto?> UpdateAsync(int id, UpdateTagDto dto);
    Task<bool> DeleteAsync(int id);
}
