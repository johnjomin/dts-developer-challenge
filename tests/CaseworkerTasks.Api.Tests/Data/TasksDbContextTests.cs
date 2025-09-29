using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CaseworkerTasks.Api.Tests.Data;

public class TasksDbContextTests
{
    private TasksDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TasksDbContext(options);
    }

    [Fact]
    public async Task CanSaveAndRetrieveTaskItem()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test task persistence",
            Description = "Making sure our database actually works",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        // Act - Save
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act - Retrieve
        var retrievedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);

        // Assert
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Id.Should().Be(task.Id);
        retrievedTask.Title.Should().Be(task.Title);
        retrievedTask.Description.Should().Be(task.Description);
        retrievedTask.Status.Should().Be(task.Status);
        retrievedTask.DueAt.Should().BeCloseTo(task.DueAt!.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CanSaveTaskWithNullDescription()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task without description",
            Description = null,
            Status = TaskStatus.InProgress,
            DueAt = null
        };

        // Act
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var retrievedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);

        // Assert
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Description.Should().BeNull();
        retrievedTask.DueAt.Should().BeNull();
    }

    [Fact]
    public async Task CanRetrieveMultipleTasks()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.ToDo },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.InProgress },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 3", Status = TaskStatus.Done }
        };

        // Act
        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();

        var allTasks = await context.Tasks.ToListAsync();

        // Assert
        allTasks.Should().HaveCount(3);
        allTasks.Select(t => t.Title).Should().Contain("Task 1", "Task 2", "Task 3");
    }
}