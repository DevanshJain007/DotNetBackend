using BackendOfReactProject.DTO;

namespace BackendOfReactProject.Services
{
    public interface IBlogPostService
    {
        Task<BlogPostResponseDto> CreatePostAsync(CreateBlogPostDto dto, int authorId);
        Task<BlogPostResponseDto?> GetPostByIdAsync(int id);
        Task<BlogPostResponseDto?> GetPostBySlugAsync(string slug);
        Task<IEnumerable<BlogPostResponseDto>> GetAllPostsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogPostResponseDto>> GetPublishedPostsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogPostResponseDto>> GetPostsByAuthorAsync(int authorId, int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogPostResponseDto>> GetPostsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
        Task<BlogPostResponseDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto, int userId);
        Task<bool> DeletePostAsync(int id, int userId);
        Task<IEnumerable<BlogPostResponseDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10);
        string GenerateSlug(string title);
    }
}
