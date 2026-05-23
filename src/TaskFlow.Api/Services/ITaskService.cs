using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponseDto>> ListAsync(
        string ownerId,
        Models.TaskStatus? status,
        TaskPriority? priority,
        int page,
        int pageSize);

    Task<TaskResponseDto?> GetAsync(string ownerId, int id);
    Task<TaskResponseDto> CreateAsync(string ownerId, CreateTaskDto dto);
    Task<TaskResponseDto?> UpdateAsync(string ownerId, int id, UpdateTaskDto dto);
    Task<bool> DeleteAsync(string ownerId, int id);
}
