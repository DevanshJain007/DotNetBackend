namespace BackendOfReactProject.Models
{
    public class BlogPostTag
    {
        public int BlogPostId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public virtual BlogPost BlogPost { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
