using BackendOfReactProject.Models;
using BackendOfReactProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendOfReactProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class TaskController : Controller
    {
       
        
            private readonly ITaskServices _taskService;

            public TaskController(ITaskServices taskService)
            {
                _taskService = taskService;
            }

            // GET: api/tasks
            [HttpGet]
            public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(tasks);
            }

            // GET: api/tasks/5
            [HttpGet("{id}")]
            public async Task<ActionResult<TaskItem>> GetTask(int id)
            {
                var task = await _taskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found.");
                }

                return Ok(task);
            }

            // POST: api/tasks
            [HttpPost]
            public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTask = await _taskService.CreateTaskAsync(task);
                return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
            }

            // PUT: api/tasks/5
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateTask(int id, TaskItem task)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTask = await _taskService.UpdateTaskAsync(id, task);

                if (updatedTask == null)
                {
                    return NotFound($"Task with ID {id} not found.");
                }

                return Ok(updatedTask);
            }

            // DELETE: api/tasks/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTask(int id)
            {
                var deleted = await _taskService.DeleteTaskAsync(id);

                if (!deleted)
                {
                    return NotFound($"Task with ID {id} not found.");
                }

                return NoContent();
            }

            // PATCH: api/tasks/5/toggle
            [HttpPatch("{id}/toggle")]
            public async Task<IActionResult> ToggleTaskCompletion(int id)
            {
                var task = await _taskService.GetTaskByIdAsync(id);

                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found.");
                }

                task.IsCompleted = !task.IsCompleted;
                var updatedTask = await _taskService.UpdateTaskAsync(id, task);

                return Ok(updatedTask);
            
        }
    }
}
