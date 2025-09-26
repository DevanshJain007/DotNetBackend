// Services/ICategoryService.cs
using BackendOfReactProject.DTO;

namespace BlogAPI.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
        string GenerateSlug(string name);
    }
}
