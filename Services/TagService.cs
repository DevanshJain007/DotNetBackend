// Services/TagService.cs
using BackendOfReactProject.Data;
using BackendOfReactProject.DTO;
using BackendOfReactProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BlogAPI.Services
{
    public class TagService : ITagService
    {
        private readonly BlogContext _context;

        public TagService(BlogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    CreatedAt = t.CreatedAt,
                    PostsCount = t.BlogPostTags.Count(pt => pt.BlogPost.IsPublished)
                })
                .ToListAsync();
        }

        public async Task<TagDto?> GetTagByIdAsync(int id)
        {
            return await _context.Tags
                .Where(t => t.Id == id)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    CreatedAt = t.CreatedAt,
                    PostsCount = t.BlogPostTags.Count(pt => pt.BlogPost.IsPublished)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TagDto?> GetTagBySlugAsync(string slug)
        {
            return await _context.Tags
                .Where(t => t.Slug == slug)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    CreatedAt = t.CreatedAt,
                    PostsCount = t.BlogPostTags.Count(pt => pt.BlogPost.IsPublished)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TagDto>> GetTagsByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Tags
                .Where(t => ids.Contains(t.Id))
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    CreatedAt = t.CreatedAt,
                    PostsCount = t.BlogPostTags.Count(pt => pt.BlogPost.IsPublished)
                })
                .ToListAsync();
        }

        public async Task<TagDto> CreateTagAsync(CreateTagDto dto)
        {
            // Check if tag name already exists
            if (await _context.Tags.AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower()))
                throw new InvalidOperationException("A tag with this name already exists");

            var slug = await GenerateUniqueSlugAsync(dto.Name);

            var tag = new Tag
            {
                Name = dto.Name,
                Slug = slug,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                CreatedAt = tag.CreatedAt,
                PostsCount = 0
            };
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags
                .Include(t => t.BlogPostTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null) return false;

            // Remove all blog post tag relationships first
            if (tag.BlogPostTags.Any())
            {
                _context.BlogPostTags.RemoveRange(tag.BlogPostTags);
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var slug = name.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = Regex.Replace(slug, @"-+", "-");

            return slug.Length > 50 ? slug.Substring(0, 50).Trim('-') : slug;
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeTagId = null)
        {
            var baseSlug = GenerateSlug(name);
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludeTagId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private async Task<bool> SlugExistsAsync(string slug, int? excludeTagId)
        {
            return await _context.Tags
                .AnyAsync(t => t.Slug == slug && (excludeTagId == null || t.Id != excludeTagId));
        }
    }
}