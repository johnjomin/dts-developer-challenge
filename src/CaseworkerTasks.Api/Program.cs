using System.ComponentModel.DataAnnotations;
using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.DTOs;
using CaseworkerTasks.Api.Middleware;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Repositories;
using CaseworkerTasks.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() {
        Title = "Caseworker Tasks API",
        Version = "v1",
        Description = "A simple task management system for caseworkers"
    });

    // Add response examples and status codes
    c.EnableAnnotations();
});

// Add problem details support
builder.Services.AddProblemDetails();

// Add database context
builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db"));

// Add repositories and services
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Seed sample data in development
    if (app.Environment.IsDevelopment())
    {
        await DatabaseSeeder.SeedAsync(context);
    }
}

// Configure the HTTP request pipeline.
// Add global exception handling middleware first
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add problem details middleware
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caseworker Tasks API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", async (TasksDbContext context) =>
{
    try
    {
        // Simple database connectivity check
        await context.Database.CanConnectAsync();

        return Results.Ok(new
        {
            Status = "Healthy",
            Service = "Caseworker Tasks API",
            Timestamp = DateTime.UtcNow,
            Database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: "Service Unhealthy"
        );
    }
})
.WithName("GetHealth")
.WithSummary("Health check endpoint")
.WithDescription("Returns the health status of the API and database connection. Use this endpoint to verify the service is running and can connect to the database.")
.Produces<object>(200, "application/json")
.Produces<object>(503, "application/problem+json")
.WithOpenApi();

// Welcome endpoint
app.MapGet("/", () => new
{
    Message = "Welcome to Caseworker Tasks API",
    Documentation = "/swagger",
    Health = "/health"
})
.WithName("GetWelcome")
.WithSummary("API welcome message")
.WithDescription("Returns welcome information and links to available endpoints. Start here to explore the API.")
.Produces<object>(200, "application/json")
.WithOpenApi();

// Task endpoints
app.MapPost("/tasks", async (CreateTaskRequest request, ITaskService taskService) =>
{
    // Validate the request
    var validationContext = new ValidationContext(request);
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        var errors = validationResults.ToDictionary(
            vr => vr.MemberNames.FirstOrDefault() ?? "General",
            vr => vr.ErrorMessage ?? "Invalid value"
        );

        return Results.ValidationProblem(errors);
    }

    var response = await taskService.CreateTaskAsync(request);
    return Results.Created($"/tasks/{response.Id}", response);
})
.WithName("CreateTask")
.WithSummary("Create a new task")
.WithDescription("Creates a new task with the provided information. The task will be assigned a unique ID and set to 'ToDo' status by default.")
.Accepts<CreateTaskRequest>("application/json")
.Produces<TaskResponse>(201, "application/json")
.ProducesValidationProblem(400)
.WithOpenApi();

app.MapGet("/tasks/{id:guid}", async (Guid id, ITaskService taskService) =>
{
    var response = await taskService.GetTaskByIdAsync(id);

    if (response == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(response);
})
.WithName("GetTaskById")
.WithSummary("Get a task by ID")
.WithDescription("Retrieves a specific task by its unique identifier. Returns 404 if the task is not found.")
.Produces<TaskResponse>(200, "application/json")
.Produces(404)
.Produces(400)
.WithOpenApi();

app.MapGet("/tasks", async (ITaskService taskService) =>
{
    var response = await taskService.GetAllTasksAsync();
    return Results.Ok(response.ToArray());
})
.WithName("GetAllTasks")
.WithSummary("Get all tasks")
.WithDescription("Retrieves all tasks sorted by due date for optimal task prioritization. Tasks with due dates appear first (ordered chronologically), followed by tasks without due dates.")
.Produces<TaskResponse[]>(200, "application/json")
.WithOpenApi();

app.MapPatch("/tasks/{id:guid}/status", async (Guid id, UpdateTaskStatusRequest request, ITaskService taskService) =>
{
    // Validate the request
    var validationContext = new ValidationContext(request);
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        var errors = validationResults.ToDictionary(
            vr => vr.MemberNames.FirstOrDefault() ?? "General",
            vr => vr.ErrorMessage ?? "Invalid value"
        );

        return Results.ValidationProblem(errors);
    }

    var response = await taskService.UpdateTaskStatusAsync(id, request);

    if (response == null)
    {
        // Could be invalid status or task not found
        if (!Enum.TryParse<TaskStatus>(request.Status, out _))
        {
            return Results.BadRequest("Invalid status. Valid statuses are: ToDo, InProgress, Done");
        }
        return Results.NotFound();
    }

    return Results.Ok(response);
})
.WithName("UpdateTaskStatus")
.WithSummary("Update task status")
.WithDescription("Updates the status of a specific task. Valid statuses are 'ToDo', 'InProgress', and 'Done'. This endpoint supports workflow transitions for task management.")
.Accepts<UpdateTaskStatusRequest>("application/json")
.Produces<TaskResponse>(200, "application/json")
.ProducesValidationProblem(400)
.Produces(404)
.WithOpenApi();

app.MapDelete("/tasks/{id:guid}", async (Guid id, ITaskService taskService) =>
{
    var success = await taskService.DeleteTaskAsync(id);

    if (!success)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
})
.WithName("DeleteTask")
.WithSummary("Delete a task")
.WithDescription("Permanently deletes a specific task by its unique identifier. This action cannot be undone.")
.Produces(204)
.Produces(404)
.Produces(400)
.WithOpenApi();

app.Run();

// Make the Program class public for testing
public partial class Program { }