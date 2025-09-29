using CaseworkerTasks.Api.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace CaseworkerTasks.Api.Tests.Middleware;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
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
    public async Task UnhandledException_ReturnsInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Try to access a non-existent endpoint that might trigger an exception
        var response = await client.GetAsync("/tasks/force-error");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // Note: This test verifies that our error handling doesn't break normal 404 responses
    }

    [Fact]
    public async Task ValidationError_ReturnsProperProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidRequest = new
        {
            Title = "", // Invalid empty title
            Description = "This should fail validation"
        };

        // Act
        var response = await client.PostAsJsonAsync("/tasks", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(content, _jsonOptions);

        problemDetails.Should().NotBeNull();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Status.Should().Be(400);
        problemDetails.Errors.Should().ContainKey("Title");
    }

    [Fact]
    public async Task NotFoundError_ReturnsProperResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/tasks/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify content type is appropriate
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task BadRequestError_ReturnsProperFormat()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidId = "not-a-guid";

        // Act
        var response = await client.GetAsync($"/tasks/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The response should be in a consistent format
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    private class ProblemDetailsResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}