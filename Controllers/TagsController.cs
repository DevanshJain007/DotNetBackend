// Controllers/TagsController.cs
using BackendOfReactProject.DTO;
using BlogAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: api/tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tags" });
            }
        }

        // GET: api/tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);

                if (tag == null)
                    return NotFound(new { message = $"Tag with ID {id} not found" });

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tag" });
            }
        }

        // POST: api/tags
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tag = await _tagService.CreateTagAsync(createTagDto);

                return CreatedAtAction(
                    nameof(GetTag),
                    new { id = tag.Id },
                    tag);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating tag" });
            }
        }

        // DELETE: api/tags/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                var deleted = await _tagService.DeleteTagAsync(id);

                if (!deleted)
                    return NotFound(new { message = $"Tag with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting tag" });
            }
        }
    }
}