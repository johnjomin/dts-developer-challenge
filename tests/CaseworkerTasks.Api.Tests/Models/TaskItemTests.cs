using CaseworkerTasks.Api.Models;
using FluentAssertions;

namespace CaseworkerTasks.Api.Tests.Models;

public class TaskItemTests
{
    [Fact]
    public void TaskItem_ShouldHaveRequiredProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Fix the coffee machine";
        var description = "The coffee machine is making weird noises again T-T";
        var status = TaskStatus.ToDo;
        var dueAt = DateTime.UtcNow.AddDays(3);

        // Act
        var task = new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            Status = status,
            DueAt = dueAt
        };

        // Assert
        task.Id.Should().Be(id);
        task.Title.Should().Be(title);
        task.Description.Should().Be(description);
        task.Status.Should().Be(status);
        task.DueAt.Should().Be(dueAt);
    }

    [Fact]
    public void TaskItem_ShouldAllowNullDescription()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Simple task",
            Description = null,
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow
        };

        // Assert
        task.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(TaskStatus.ToDo)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public void TaskItem_ShouldSupportAllStatuses(TaskStatus status)
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test task",
            Status = status,
            DueAt = DateTime.UtcNow
        };

        // Assert
        task.Status.Should().Be(status);
    }
}