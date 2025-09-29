using System.ComponentModel.DataAnnotations;
using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.DTOs;
using CaseworkerTasks.Api.Models;
using CaseworkerTasks.Api.Repositories;
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
});

// Add database context
builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db"));

// Add repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
.WithDescription("Returns the health status of the API and database connection")
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
.WithDescription("Returns welcome information and available endpoints")
.WithOpenApi();

// Task endpoints
app.MapPost("/tasks", async (CreateTaskRequest request, ITaskRepository repository) =>
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

    // Create the task
    var task = new TaskItem
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description,
        Status = TaskStatus.ToDo,
        DueAt = request.DueAt
    };

    var createdTask = await repository.CreateAsync(task);
    var response = TaskResponse.FromTaskItem(createdTask);

    return Results.Created($"/tasks/{response.Id}", response);
})
.WithName("CreateTask")
.WithSummary("Create a new task")
.WithDescription("Creates a new task with the provided information")
.WithOpenApi();

app.Run();

// Make the Program class public for testing
public partial class Program { }