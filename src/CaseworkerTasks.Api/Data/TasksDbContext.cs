using CaseworkerTasks.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseworkerTasks.Api.Data;

public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string for readability
            entity.Property(e => e.DueAt)
                .HasColumnType("datetime"); // Explicit datetime type for SQLite
        });
    }
}