// Services/ITagService.cs
using BackendOfReactProject.DTO;

namespace BlogAPI.Services
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<TagDto?> GetTagByIdAsync(int id);
        Task<TagDto> CreateTagAsync(CreateTagDto dto);
        Task<bool> DeleteTagAsync(int id);
        Task<TagDto?> GetTagBySlugAsync(string slug);
        Task<IEnumerable<TagDto>> GetTagsByIdsAsync(IEnumerable<int> ids);
        string GenerateSlug(string name);
    }
}