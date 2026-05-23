using FluentAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;
using Xunit;

namespace TaskFlow.Tests;

public class ValidationTests
{
    private readonly CreateTaskDtoValidator _taskV = new();
    private readonly RegisterDtoValidator _registerV = new();
    private readonly LoginDtoValidator _loginV = new();

    [Fact]
    public void CreateTask_rejects_empty_title()
    {
        var dto = new CreateTaskDto("", null, TaskPriority.Low, null);
        _taskV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateTask_rejects_past_due_date()
    {
        var dto = new CreateTaskDto("ok", null, TaskPriority.Low, DateTime.UtcNow.AddDays(-7));
        _taskV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void CreateTask_accepts_future_due_date()
    {
        var dto = new CreateTaskDto("ok", null, TaskPriority.Low, DateTime.UtcNow.AddDays(7));
        _taskV.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Register_rejects_short_password()
    {
        var dto = new RegisterDto("a@b.com", "short", "Name");
        _registerV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Register_requires_uppercase_in_password()
    {
        var dto = new RegisterDto("a@b.com", "alllower1", "Name");
        _registerV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Register_requires_digit_in_password()
    {
        var dto = new RegisterDto("a@b.com", "NoDigits", "Name");
        _registerV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_rejects_bad_email()
    {
        var dto = new LoginDto("not-an-email", "Password1");
        _loginV.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_accepts_valid_input()
    {
        var dto = new LoginDto("a@b.com", "Password1");
        _loginV.TestValidate(dto).IsValid.Should().BeTrue();
    }
}
