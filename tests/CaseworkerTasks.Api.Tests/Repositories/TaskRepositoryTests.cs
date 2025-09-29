using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CaseworkerTasks.Api.Tests.Repositories;

public class TaskRepositoryTests
{
    private TasksDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TasksDbContext(options);
    }

    private TaskRepository CreateRepository(TasksDbContext context)
    {
        return new TaskRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsTask()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test task for retrieval",
            Description = "This task should be found",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
        result.Description.Should().Be(task.Description);
        result.Status.Should().Be(task.Status);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTask_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleTasks_ReturnsAllOrderedByDueDate()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 3", Status = TaskStatus.ToDo, DueAt = DateTime.UtcNow.AddDays(3) },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.InProgress, DueAt = DateTime.UtcNow.AddDays(1) },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.Done, DueAt = DateTime.UtcNow.AddDays(2) },
            new TaskItem { Id = Guid.NewGuid(), Title = "No Due Date", Status = TaskStatus.ToDo, DueAt = null }
        };

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(4);
        // Tasks with due dates should come first, ordered by due date
        // Tasks without due dates should come last
        var resultList = result.ToList();
        resultList[0].Title.Should().Be("Task 1"); // Due in 1 day
        resultList[1].Title.Should().Be("Task 2"); // Due in 2 days
        resultList[2].Title.Should().Be("Task 3"); // Due in 3 days
        resultList[3].Title.Should().Be("No Due Date"); // No due date
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidTask_AddsTaskToDatabase()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "New task to create",
            Description = "This task will be created",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var result = await repository.CreateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);

        var savedTask = await context.Tasks.FindAsync(task.Id);
        savedTask.Should().NotBeNull();
        savedTask!.Title.Should().Be(task.Title);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesTask()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Original title",
            Description = "Original description",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Modify the task
        task.Title = "Updated title";
        task.Status = TaskStatus.InProgress;
        task.Description = "Updated description";

        // Act
        var result = await repository.UpdateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated title");
        result.Status.Should().Be(TaskStatus.InProgress);
        result.Description.Should().Be("Updated description");

        // Verify in database
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        updatedTask!.Title.Should().Be("Updated title");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTask_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var nonExistentTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Non-existent task",
            Status = TaskStatus.ToDo
        };

        // Act
        var result = await repository.UpdateAsync(nonExistentTask);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingTask_RemovesTaskFromDatabase()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task to delete",
            Status = TaskStatus.ToDo
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.DeleteAsync(task.Id);

        // Assert
        result.Should().BeTrue();

        // Verify task is gone
        var deletedTask = await context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTask_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = CreateRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }
}