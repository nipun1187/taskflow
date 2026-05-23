using FluentValidation;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Dtos;

public record CreateTaskDto(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate);

public record UpdateTaskDto(
    string Title,
    string? Description,
    Models.TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);

public record TaskResponseDto(
    int Id,
    string Title,
    string? Description,
    Models.TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.AddDays(-1))
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date cannot be in the past.");
    }
}

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
