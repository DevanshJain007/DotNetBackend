using BackendOfReactProject.Data;
using BackendOfReactProject.DTO;
using BackendOfReactProject.Models;

using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BlogAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly BlogContext _context;

        public CategoryService(BlogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    CreatedAt = c.CreatedAt,
                    PostsCount = c.BlogPosts.Count(p => p.IsPublished)
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    CreatedAt = c.CreatedAt,
                    PostsCount = c.BlogPosts.Count(p => p.IsPublished)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .Where(c => c.Slug == slug)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    CreatedAt = c.CreatedAt,
                    PostsCount = c.BlogPosts.Count(p => p.IsPublished)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Check if category name already exists
            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
                throw new InvalidOperationException("A category with this name already exists");

            var slug = await GenerateUniqueSlugAsync(dto.Name);

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                CreatedAt = category.CreatedAt,
                PostsCount = 0
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            // Check if another category has this name
            if (await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower()))
                throw new InvalidOperationException("Another category with this name already exists");

            category.Name = dto.Name;
            category.Description = dto.Description;

            // Update slug if name changed
            if (category.Name != dto.Name)
            {
                category.Slug = await GenerateUniqueSlugAsync(dto.Name, id);
            }

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                CreatedAt = category.CreatedAt,
                PostsCount = await _context.BlogPosts.CountAsync(p => p.CategoryId == id && p.IsPublished)
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.BlogPosts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return false;

            // Check if category has posts
            if (category.BlogPosts.Any())
                throw new InvalidOperationException("Cannot delete category that contains blog posts. Move or delete the posts first.");

            _context.Categories.Remove(category);
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

            return slug.Length > 100 ? slug.Substring(0, 100).Trim('-') : slug;
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeCategoryId = null)
        {
            var baseSlug = GenerateSlug(name);
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludeCategoryId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private async Task<bool> SlugExistsAsync(string slug, int? excludeCategoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Slug == slug && (excludeCategoryId == null || c.Id != excludeCategoryId));
        }
    }
}
