using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext db, ILogger<TaskService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskResponseDto>> ListAsync(
        string ownerId,
        Models.TaskStatus? status,
        TaskPriority? priority,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Tasks.AsNoTracking().Where(t => t.OwnerId == ownerId);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<TaskResponseDto?> GetAsync(string ownerId, int id)
    {
        var t = await _db.Tasks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);
        return t is null ? null : ToDto(t);
    }

    public async Task<TaskResponseDto> CreateAsync(string ownerId, CreateTaskDto dto)
    {
        var entity = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            OwnerId = ownerId,
            Status = Models.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} created for user {UserId}", entity.Id, ownerId);
        return ToDto(entity);
    }

    public async Task<TaskResponseDto?> UpdateAsync(string ownerId, int id, UpdateTaskDto dto)
    {
        var entity = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);
        if (entity is null) return null;

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim();
        entity.Priority = dto.Priority;
        entity.DueDate = dto.DueDate;

        if (entity.Status != dto.Status)
        {
            entity.Status = dto.Status;
            entity.CompletedAt = dto.Status == Models.TaskStatus.Completed ? DateTime.UtcNow : null;
        }

        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(string ownerId, int id)
    {
        var entity = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);
        if (entity is null) return false;
        _db.Tasks.Remove(entity);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, ownerId);
        return true;
    }

    private static TaskResponseDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority,
        t.DueDate, t.CreatedAt, t.CompletedAt);
}
