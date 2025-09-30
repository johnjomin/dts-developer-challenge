using CaseworkerTasks.Api.DTOs;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Repositories;
using CaseworkerTasks.Api.Services;
using FluentAssertions;
using NSubstitute;

namespace CaseworkerTasks.Api.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _mockRepository;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _mockRepository = Substitute.For<ITaskRepository>();
        _service = new TaskService(_mockRepository);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ReturnsTaskResponse()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test task",
            Description = "Test description",
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        var createdTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.ToDo,
            DueAt = request.DueAt
        };

        _mockRepository.CreateAsync(Arg.Any<TaskItem>()).Returns(createdTask);

        // Act
        var result = await _service.CreateTaskAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Status.Should().Be("ToDo");
        result.DueAt.Should().Be(request.DueAt);

        await _mockRepository.Received(1).CreateAsync(Arg.Is<TaskItem>(t =>
            t.Title == request.Title &&
            t.Description == request.Description &&
            t.Status == TaskStatus.ToDo));
    }

    [Fact]
    public async Task GetTaskByIdAsync_ExistingTask_ReturnsTaskResponse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Existing task",
            Description = "Task description",
            Status = TaskStatus.InProgress,
            DueAt = DateTime.UtcNow.AddDays(2)
        };

        _mockRepository.GetByIdAsync(taskId).Returns(task);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Existing task");
        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task GetTaskByIdAsync_NonExistentTask_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllTasksAsync_WithTasks_ReturnsTaskResponses()
    {
        // Arrange
        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.ToDo },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.Done }
        };

        _mockRepository.GetAllAsync().Returns(tasks);

        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Task 1");
        result.Last().Title.Should().Be("Task 2");
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ValidRequest_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Task to update",
            Status = TaskStatus.ToDo
        };

        var request = new UpdateTaskStatusRequest { Status = "InProgress" };

        _mockRepository.GetByIdAsync(taskId).Returns(existingTask);
        _mockRepository.UpdateAsync(Arg.Any<TaskItem>()).Returns(existingTask);

        // Act
        var result = await _service.UpdateTaskStatusAsync(taskId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("InProgress");

        await _mockRepository.Received(1).UpdateAsync(Arg.Is<TaskItem>(t => t.Status == TaskStatus.InProgress));
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_InvalidStatus_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskStatusRequest { Status = "InvalidStatus" };

        // Act
        var result = await _service.UpdateTaskStatusAsync(taskId, request);

        // Assert
        result.Should().BeNull();
        await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_NonExistentTask_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskStatusRequest { Status = "InProgress" };

        _mockRepository.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act
        var result = await _service.UpdateTaskStatusAsync(taskId, request);

        // Assert
        result.Should().BeNull();
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>());
    }

    [Fact]
    public async Task DeleteTaskAsync_ExistingTask_ReturnsTrue()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockRepository.DeleteAsync(taskId).Returns(true);

        // Act
        var result = await _service.DeleteTaskAsync(taskId);

        // Assert
        result.Should().BeTrue();
        await _mockRepository.Received(1).DeleteAsync(taskId);
    }

    [Fact]
    public async Task DeleteTaskAsync_NonExistentTask_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockRepository.DeleteAsync(taskId).Returns(false);

        // Act
        var result = await _service.DeleteTaskAsync(taskId);

        // Assert
        result.Should().BeFalse();
    }
}