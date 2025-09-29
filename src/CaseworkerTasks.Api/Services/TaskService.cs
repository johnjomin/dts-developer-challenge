using CaseworkerTasks.Api.DTOs;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Repositories;

namespace CaseworkerTasks.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.ToDo,
            DueAt = request.DueAt
        };

        var createdTask = await _repository.CreateAsync(task);
        return TaskResponse.FromTaskItem(createdTask);
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task == null ? null : TaskResponse.FromTaskItem(task);
    }

    public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync()
    {
        var tasks = await _repository.GetAllAsync();
        return tasks.Select(TaskResponse.FromTaskItem);
    }

    public async Task<TaskResponse?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusRequest request)
    {
        // Validate status value
        if (!Enum.TryParse<TaskStatus>(request.Status, out var newStatus))
        {
            return null;
        }

        // Get the existing task
        var existingTask = await _repository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return null;
        }

        // Update only the status
        existingTask.Status = newStatus;
        var updatedTask = await _repository.UpdateAsync(existingTask);

        return updatedTask == null ? null : TaskResponse.FromTaskItem(updatedTask);
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
}