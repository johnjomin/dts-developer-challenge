using CaseworkerTasks.Api.Models;

namespace CaseworkerTasks.Api.DTOs;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }

    public static TaskResponse FromTaskItem(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueAt = task.DueAt
        };
    }
}