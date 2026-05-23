using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;
using Xunit;
using TStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Tests;

public class TaskServiceTests
{
    private static AppDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    private static TaskService NewSvc(AppDbContext db) =>
        new(db, NullLogger<TaskService>.Instance);

    [Fact]
    public async Task CreateAsync_persists_task_for_owner()
    {
        using var db = NewDb();
        var svc = NewSvc(db);

        var dto = new CreateTaskDto("Write report", "BCA final", TaskPriority.High, null);
        var result = await svc.CreateAsync("user-1", dto);

        result.Title.Should().Be("Write report");
        result.Status.Should().Be(TStatus.Pending);
        (await db.Tasks.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ListAsync_returns_only_owner_tasks()
    {
        using var db = NewDb();
        db.Tasks.AddRange(
            new TaskItem { Title = "A", OwnerId = "u1" },
            new TaskItem { Title = "B", OwnerId = "u2" },
            new TaskItem { Title = "C", OwnerId = "u1" });
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        var list = await svc.ListAsync("u1", null, null, 1, 20);

        list.Should().HaveCount(2);
        list.Select(t => t.Title).Should().BeEquivalentTo(new[] { "A", "C" });
    }

    [Fact]
    public async Task ListAsync_filters_by_status()
    {
        using var db = NewDb();
        db.Tasks.AddRange(
            new TaskItem { Title = "p", OwnerId = "u1", Status = TStatus.Pending },
            new TaskItem { Title = "c", OwnerId = "u1", Status = TStatus.Completed });
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        var done = await svc.ListAsync("u1", TStatus.Completed, null, 1, 20);
        done.Should().ContainSingle(x => x.Title == "c");
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_owner_mismatch()
    {
        using var db = NewDb();
        var t = new TaskItem { Title = "x", OwnerId = "owner" };
        db.Tasks.Add(t);
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        var dto = new UpdateTaskDto("y", null, TStatus.Pending, TaskPriority.Low, null);
        var result = await svc.UpdateAsync("intruder", t.Id, dto);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_sets_CompletedAt_when_status_becomes_completed()
    {
        using var db = NewDb();
        var t = new TaskItem { Title = "x", OwnerId = "u", Status = TStatus.InProgress };
        db.Tasks.Add(t);
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        var dto = new UpdateTaskDto("x", null, TStatus.Completed, TaskPriority.Low, null);
        var updated = await svc.UpdateAsync("u", t.Id, dto);

        updated!.Status.Should().Be(TStatus.Completed);
        updated.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_removes_task()
    {
        using var db = NewDb();
        var t = new TaskItem { Title = "x", OwnerId = "u" };
        db.Tasks.Add(t);
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        (await svc.DeleteAsync("u", t.Id)).Should().BeTrue();
        (await db.Tasks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_not_found()
    {
        using var db = NewDb();
        var svc = NewSvc(db);
        (await svc.DeleteAsync("u", 999)).Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_paginates_correctly()
    {
        using var db = NewDb();
        for (int i = 0; i < 25; i++)
            db.Tasks.Add(new TaskItem { Title = $"t{i}", OwnerId = "u" });
        await db.SaveChangesAsync();

        var svc = NewSvc(db);
        var page1 = await svc.ListAsync("u", null, null, 1, 10);
        var page3 = await svc.ListAsync("u", null, null, 3, 10);

        page1.Should().HaveCount(10);
        page3.Should().HaveCount(5);
    }
}
