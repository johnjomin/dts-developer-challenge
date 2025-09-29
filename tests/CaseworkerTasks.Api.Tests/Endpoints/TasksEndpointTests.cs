using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CaseworkerTasks.Api.Tests.Endpoints;

public class TasksEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public TasksEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<TasksDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            });
        });

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task POST_Tasks_ValidTask_ReturnsCreatedTask()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createTaskRequest = new
        {
            Title = "Fix the printer",
            Description = "The office printer is jamming constantly",
            DueAt = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var createdTask = JsonSerializer.Deserialize<TaskResponse>(content, _jsonOptions);

        createdTask.Should().NotBeNull();
        createdTask!.Id.Should().NotBeEmpty();
        createdTask.Title.Should().Be("Fix the printer");
        createdTask.Description.Should().Be("The office printer is jamming constantly");
        createdTask.Status.Should().Be("ToDo");
        createdTask.DueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(2), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task POST_Tasks_ValidTaskWithoutDescription_ReturnsCreatedTask()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createTaskRequest = new
        {
            Title = "Simple task",
            DueAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var createdTask = JsonSerializer.Deserialize<TaskResponse>(content, _jsonOptions);

        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Simple task");
        createdTask.Description.Should().BeNull();
        createdTask.Status.Should().Be("ToDo");
    }

    [Fact]
    public async Task POST_Tasks_ValidTaskWithoutDueDate_ReturnsCreatedTask()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createTaskRequest = new
        {
            Title = "Task without deadline",
            Description = "This task can be done anytime"
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var createdTask = JsonSerializer.Deserialize<TaskResponse>(content, _jsonOptions);

        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Task without deadline");
        createdTask.DueAt.Should().BeNull();
    }

    [Fact]
    public async Task POST_Tasks_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createTaskRequest = new
        {
            Title = "",
            Description = "This should fail validation"
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Title is required");
    }

    [Fact]
    public async Task POST_Tasks_TitleTooLong_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var longTitle = new string('A', 201); // 201 characters
        var createTaskRequest = new
        {
            Title = longTitle,
            Description = "This should fail validation due to title length"
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("200 characters");
    }

    [Fact]
    public async Task POST_Tasks_PastDueDate_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createTaskRequest = new
        {
            Title = "Task with past due date",
            Description = "This should fail validation",
            DueAt = DateTime.UtcNow.AddDays(-1) // Yesterday
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Due date must be in the future");
    }

    [Fact]
    public async Task POST_Tasks_DescriptionTooLong_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var longDescription = new string('B', 1001); // 1001 characters
        var createTaskRequest = new
        {
            Title = "Valid title",
            Description = longDescription
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("1000 characters");
    }

    [Fact]
    public async Task GET_Tasks_ById_ExistingTask_ReturnsTask()
    {
        // Arrange
        var client = _factory.CreateClient();

        // First create a task
        var createTaskRequest = new
        {
            Title = "Task to retrieve",
            Description = "This task should be found by ID",
            DueAt = DateTime.UtcNow.AddDays(3)
        };

        var createResponse = await client.PostAsJsonAsync("/tasks", createTaskRequest);
        var createdTaskContent = await createResponse.Content.ReadAsStringAsync();
        var createdTask = JsonSerializer.Deserialize<TaskResponse>(createdTaskContent, _jsonOptions);

        // Act
        var response = await client.GetAsync($"/tasks/{createdTask!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var retrievedTask = JsonSerializer.Deserialize<TaskResponse>(content, _jsonOptions);

        retrievedTask.Should().NotBeNull();
        retrievedTask!.Id.Should().Be(createdTask.Id);
        retrievedTask.Title.Should().Be("Task to retrieve");
        retrievedTask.Description.Should().Be("This task should be found by ID");
        retrievedTask.Status.Should().Be("ToDo");
        retrievedTask.DueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GET_Tasks_ById_NonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/tasks/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_Tasks_ById_InvalidGuid_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidId = "not-a-guid";

        // Act
        var response = await client.GetAsync($"/tasks/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private class TaskResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueAt { get; set; }
    }
}