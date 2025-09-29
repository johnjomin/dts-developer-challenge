using CaseworkerTasks.Api.DTOs;

namespace CaseworkerTasks.Api.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse?> GetTaskByIdAsync(Guid id);
    Task<IEnumerable<TaskResponse>> GetAllTasksAsync();
    Task<TaskResponse?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusRequest request);
    Task<bool> DeleteTaskAsync(Guid id);
}