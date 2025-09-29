using System.ComponentModel.DataAnnotations;

namespace CaseworkerTasks.Api.DTOs;

public class UpdateTaskStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}