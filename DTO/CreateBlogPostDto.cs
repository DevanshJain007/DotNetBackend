using System.ComponentModel.DataAnnotations;

namespace BackendOfReactProject.DTO
{
    public class CreateBlogPostDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Excerpt { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FeaturedImage { get; set; }

        public int? CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new();
        public bool IsPublished { get; set; } = false;
    }

    public class UpdateBlogPostDto : CreateBlogPostDto
    {
    }

    public class BlogPostResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Author info
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;

        // Category info
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Tags
        public List<TagDto> Tags { get; set; } = new();

        // Comments count
        public int CommentsCount { get; set; }
    }
}
