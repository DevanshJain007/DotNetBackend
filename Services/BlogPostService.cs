using BackendOfReactProject.Data;
using BackendOfReactProject.DTO;
using BackendOfReactProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BackendOfReactProject.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly BlogContext _context;

        public BlogPostService(BlogContext context)
        {
            _context = context;
        }
        public async Task<BlogPostResponseDto> CreatePostAsync(CreateBlogPostDto dto, int authorId)
        {
            var slug = await GenerateUniqueSlugAsync(dto.Title);

            var post = new BlogPost
            {
                Title = dto.Title,
                Slug = slug,
                Excerpt = dto.Excerpt,
                Content = dto.Content,
                FeaturedImage = dto.FeaturedImage,
                IsPublished = dto.IsPublished,
                AuthorId = authorId,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = dto.IsPublished ? DateTime.UtcNow : null
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            // Add tags
            if (dto.TagIds.Any())
            {
                var postTags = dto.TagIds.Select(tagId => new BlogPostTag
                {
                    BlogPostId = post.Id,
                    TagId = tagId
                });

                _context.BlogPostTags.AddRange(postTags);
                await _context.SaveChangesAsync();
            }

            return await GetPostByIdAsync(post.Id) ?? throw new InvalidOperationException("Failed to retrieve created post");
        }

        public async Task<BlogPostResponseDto?> GetPostByIdAsync(int id)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.Id == id)
                .Select(p => MapToResponseDto(p))
                .FirstOrDefaultAsync();
        }

        public async Task<BlogPostResponseDto?> GetPostBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.Slug == slug)
                .Select(p => MapToResponseDto(p))
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<BlogPostResponseDto>> GetAllPostsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPostResponseDto>> GetPublishedPostsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.IsPublished && p.PublishedAt <= DateTime.UtcNow)
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPostResponseDto>> GetPostsByAuthorAsync(int authorId, int page = 1, int pageSize = 10)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPostResponseDto>> GetPostsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public async Task<BlogPostResponseDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto, int userId)
        {
            var post = await _context.BlogPosts
                .Include(p => p.BlogPostTags)
                .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId);

            if (post == null) return null;

            // Update basic properties
            post.Title = dto.Title;
            post.Excerpt = dto.Excerpt;
            post.Content = dto.Content;
            post.FeaturedImage = dto.FeaturedImage;
            post.CategoryId = dto.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;

            // Handle publishing status
            if (dto.IsPublished && !post.IsPublished)
            {
                post.IsPublished = true;
                post.PublishedAt = DateTime.UtcNow;
            }
            else if (!dto.IsPublished && post.IsPublished)
            {
                post.IsPublished = false;
                post.PublishedAt = null;
            }

            // Update slug if title changed
            if (post.Title != dto.Title)
            {
                post.Slug = await GenerateUniqueSlugAsync(dto.Title, post.Id);
            }

            // Update tags
            _context.BlogPostTags.RemoveRange(post.BlogPostTags);
            if (dto.TagIds.Any())
            {
                var newPostTags = dto.TagIds.Select(tagId => new BlogPostTag
                {
                    BlogPostId = post.Id,
                    TagId = tagId
                });
                _context.BlogPostTags.AddRange(newPostTags);
            }

            await _context.SaveChangesAsync();
            return await GetPostByIdAsync(post.Id);
        }

        public async Task<bool> DeletePostAsync(int id, int userId)
        {
            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId);

            if (post == null) return false;

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BlogPostResponseDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10)
        {
            var searchTerm = query.ToLower();

            return await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                .Where(p => p.IsPublished &&
                           (p.Title.ToLower().Contains(searchTerm) ||
                            p.Content.ToLower().Contains(searchTerm) ||
                            (p.Excerpt != null && p.Excerpt.ToLower().Contains(searchTerm))))
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            var slug = title.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = Regex.Replace(slug, @"-+", "-");

            return slug.Length > 100 ? slug.Substring(0, 100).Trim('-') : slug;
        }

        private async Task<string> GenerateUniqueSlugAsync(string title, int? excludePostId = null)
        {
            var baseSlug = GenerateSlug(title);
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludePostId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private async Task<bool> SlugExistsAsync(string slug, int? excludePostId)
        {
            return await _context.BlogPosts
                .AnyAsync(p => p.Slug == slug && (excludePostId == null || p.Id != excludePostId));
        }

        private static BlogPostResponseDto MapToResponseDto(BlogPost post)
        {
            return new BlogPostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Content = post.Content,
                FeaturedImage = post.FeaturedImage,
                IsPublished = post.IsPublished,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                AuthorId = post.AuthorId,
                AuthorName = $"{post.Author.FirstName} {post.Author.LastName}".Trim(),
                AuthorUsername = post.Author.Username,
                CategoryId = post.CategoryId,
                CategoryName = post.Category?.Name,
                Tags = post.BlogPostTags.Select(pt => new TagDto
                {
                    Id = pt.Tag.Id,
                    Name = pt.Tag.Name,
                    Slug = pt.Tag.Slug,
                    CreatedAt = pt.Tag.CreatedAt
                }).ToList(),
                CommentsCount = post.Comments.Count(c => c.IsApproved)
            };
        }
    }
}
