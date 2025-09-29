using System.ComponentModel.DataAnnotations;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Validation;
using FluentAssertions;

namespace CaseworkerTasks.Api.Tests.Validation;

public class TaskItemValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void ValidTaskItem_ShouldPassValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Valid task title",
            Description = "This is a perfectly valid task",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_WithEmptyTitle_ShouldFailValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "",
            Status = TaskStatus.ToDo
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Title is required");
    }

    [Fact]
    public void TaskItem_WithNullTitle_ShouldFailValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = null!,
            Status = TaskStatus.ToDo
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Title is required");
    }

    [Fact]
    public void TaskItem_WithTitleTooLong_ShouldFailValidation()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = longTitle,
            Status = TaskStatus.ToDo
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("200 characters");
    }

    [Fact]
    public void TaskItem_WithPastDueDate_ShouldFailValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task with past due date",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddDays(-1) // Yesterday
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("Due date must be in the future");
    }

    [Fact]
    public void TaskItem_WithNullDueDate_ShouldPassValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task without due date",
            Status = TaskStatus.ToDo,
            DueAt = null
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_WithFutureDueDate_ShouldPassValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task with future due date",
            Status = TaskStatus.ToDo,
            DueAt = DateTime.UtcNow.AddMinutes(1) // Just 1 minute in the future
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_WithDescriptionTooLong_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('B', 1001); // 1001 characters
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Valid title",
            Description = longDescription,
            Status = TaskStatus.ToDo
        };

        // Act
        var results = ValidateModel(task);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain("1000 characters");
    }
}