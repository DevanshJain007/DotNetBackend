// Controllers/BlogPostsController.cs
using BackendOfReactProject.DTO;
using BackendOfReactProject.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;

        public BlogPostsController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        // GET: api/blogposts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool publishedOnly = true)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var posts = publishedOnly
                    ? await _blogPostService.GetPublishedPostsAsync(page, pageSize)
                    : await _blogPostService.GetAllPostsAsync(page, pageSize);

                return Ok(new
                {
                    data = posts,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        hasMore = posts.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving posts" });
            }
        }

        // GET: api/blogposts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostResponseDto>> GetPost(int id)
        {
            try
            {
                var post = await _blogPostService.GetPostByIdAsync(id);

                if (post == null)
                    return NotFound(new { message = $"Post with ID {id} not found" });

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving post" });
            }
        }

        // GET: api/blogposts/slug/my-post-slug
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<BlogPostResponseDto>> GetPostBySlug(string slug)
        {
            try
            {
                var post = await _blogPostService.GetPostBySlugAsync(slug);

                if (post == null)
                    return NotFound(new { message = $"Post with slug '{slug}' not found" });

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving post" });
            }
        }

        // GET: api/blogposts/search?q=searchterm
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> SearchPosts(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "Search query is required" });

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var posts = await _blogPostService.SearchPostsAsync(q, page, pageSize);

                return Ok(new
                {
                    data = posts,
                    searchTerm = q,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        hasMore = posts.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching posts" });
            }
        }

        // GET: api/blogposts/author/5
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> GetPostsByAuthor(
            int authorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var posts = await _blogPostService.GetPostsByAuthorAsync(authorId, page, pageSize);

                return Ok(new
                {
                    data = posts,
                    authorId = authorId,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        hasMore = posts.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving author's posts" });
            }
        }

        // GET: api/blogposts/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> GetPostsByCategory(
            int categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var posts = await _blogPostService.GetPostsByCategoryAsync(categoryId, page, pageSize);

                return Ok(new
                {
                    data = posts,
                    categoryId = categoryId,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        hasMore = posts.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving category posts" });
            }
        }

        // POST: api/blogposts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BlogPostResponseDto>> CreatePost([FromBody] CreateBlogPostDto createPostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var post = await _blogPostService.CreatePostAsync(createPostDto, userId.Value);

                return CreatedAtAction(
                    nameof(GetPost),
                    new { id = post.Id },
                    post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating post" });
            }
        }

        // PUT: api/blogposts/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<BlogPostResponseDto>> UpdatePost(int id, [FromBody] UpdateBlogPostDto updatePostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var updatedPost = await _blogPostService.UpdatePostAsync(id, updatePostDto, userId.Value);

                if (updatedPost == null)
                    return NotFound(new { message = "Post not found or you don't have permission to edit it" });

                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating post" });
            }
        }

        // DELETE: api/blogposts/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var deleted = await _blogPostService.DeletePostAsync(id, userId.Value);

                if (!deleted)
                    return NotFound(new { message = "Post not found or you don't have permission to delete it" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting post" });
            }
        }

        // GET: api/blogposts/my-posts
        [HttpGet("my-posts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BlogPostResponseDto>>> GetMyPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var posts = await _blogPostService.GetPostsByAuthorAsync(userId.Value, page, pageSize);

                return Ok(new
                {
                    data = posts,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        hasMore = posts.Count() == pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving your posts" });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
