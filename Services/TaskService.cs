using BackendOfReactProject.Models;

namespace BackendOfReactProject.Services
{
    public class TaskService : ITaskServices
    {
        //first making a db which is present only not actual db 
        private static List<TaskItem> _tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Learn ASP.NET", Description = "Study Web API fundamentals", IsCompleted = false },
            new TaskItem { Id = 2, Title = "Build React App", Description = "Create frontend for task manager", IsCompleted = false },
            new TaskItem { Id = 3, Title = "Setup Database", Description = "Configure Entity Framework", IsCompleted = true }
        };
        private static int _nextId = 4;


        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            task.Id = _nextId++;
            task.CreatedDate = DateTime.Now;
            _tasks.Add(task);
            return await Task.FromResult(task);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return await Task.FromResult(false);

            _tasks.Remove(task);
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await Task.FromResult(_tasks.OrderByDescending(t => t.CreatedDate));
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return await Task.FromResult(task);
        }

        public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == id);
            if (existingTask == null)
                return await Task.FromResult<TaskItem?>(null);

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;

            return await Task.FromResult(existingTask);
        }
    }
}
