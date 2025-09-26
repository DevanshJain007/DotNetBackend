using System.ComponentModel.DataAnnotations;

namespace BackendOfReactProject.DTO
{
    public class CreateCategoryDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class CategoryDto : CreateCategoryDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostsCount { get; set; }
    }
}
