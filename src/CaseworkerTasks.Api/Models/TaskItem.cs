using System.ComponentModel.DataAnnotations;
using CaseworkerTasks.Api.Validation;

namespace CaseworkerTasks.Api.Models;

public class TaskItem
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required]
    public TaskStatus Status { get; set; }

    [FutureDate]
    public DateTime? DueAt { get; set; }
}