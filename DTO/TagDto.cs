using System.ComponentModel.DataAnnotations;

namespace BackendOfReactProject.DTO
{
    public class CreateTagDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public class TagDto : CreateTagDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostsCount { get; set; }
    }
}
