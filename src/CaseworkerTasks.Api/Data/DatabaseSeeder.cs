using CaseworkerTasks.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseworkerTasks.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TasksDbContext context)
    {
        // Only seed if database is empty
        if (await context.Tasks.AnyAsync())
        {
            return;
        }

        var sampleTasks = new[]
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Urgent: Review client case files",
                Description = "Review and update case files for upcoming court hearing. Ensure all documentation is complete and accurate.",
                Status = TaskStatus.ToDo,
                DueAt = DateTime.UtcNow.AddDays(1)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Follow up with family services",
                Description = "Contact family services regarding placement options for the Johnson family case",
                Status = TaskStatus.InProgress,
                DueAt = DateTime.UtcNow.AddDays(3)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Complete monthly report",
                Description = "Finalize and submit monthly caseload report to supervisor",
                Status = TaskStatus.Done,
                DueAt = null
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Schedule home visit",
                Description = "Arrange home visit for welfare check on the Anderson case",
                Status = TaskStatus.ToDo,
                DueAt = DateTime.UtcNow.AddDays(5)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Attend training session",
                Description = "Mandatory training on new case management procedures",
                Status = TaskStatus.ToDo,
                DueAt = DateTime.UtcNow.AddDays(7)
            }
        };

        context.Tasks.AddRange(sampleTasks);
        await context.SaveChangesAsync();
    }
}