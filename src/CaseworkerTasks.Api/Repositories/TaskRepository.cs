using CaseworkerTasks.Api.Data;
using CaseworkerTasks.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseworkerTasks.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _context;

    public TaskRepository(TasksDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
            .OrderBy(t => t.DueAt.HasValue ? t.DueAt : DateTime.MaxValue) // Tasks with due dates first, then tasks without
            .ThenBy(t => t.DueAt) // Then order by actual due date
            .ToListAsync();
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        var existingTask = await _context.Tasks.FindAsync(task.Id);
        if (existingTask == null)
        {
            return null;
        }

        // Update properties
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.DueAt = task.DueAt;

        await _context.SaveChangesAsync();
        return existingTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}