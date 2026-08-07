using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Learning.Dtos;
using Platform.Application.Features.Learning.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Learning.Commands;

public sealed class CreateLessonHandler : IRequestHandler<CreateLessonCommand, Result<LessonResponse>>
{
    private readonly IRepository<CourseModule> _modules;
    private readonly IRepository<Lesson> _lessons;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateLessonHandler(
        IRepository<CourseModule> modules,
        IRepository<Lesson> lessons,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _modules = modules;
        _lessons = lessons;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<LessonResponse>> Handle(CreateLessonCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<LessonResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var module = await _modules.GetByIdAsync(request.ModuleId, ct);
        if (module is null)
            return Result<LessonResponse>.Failure(Error.NotFound("modules.not_found", $"Module {request.ModuleId} not found."));

        var now = _clock.UtcNow.UtcDateTime;
        var lesson = new Lesson
        {
            ModuleId = request.ModuleId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Duration = request.Duration,
            Order = request.Order,
            IsPublished = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _lessons.AddAsync(lesson, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<LessonResponse>.Success(lesson.ToResponse());
    }
}

public sealed class UpdateLessonHandler : IRequestHandler<UpdateLessonCommand, Result<LessonResponse>>
{
    private readonly IRepository<Lesson> _lessons;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public UpdateLessonHandler(
        IRepository<Lesson> lessons,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _lessons = lessons;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<LessonResponse>> Handle(UpdateLessonCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<LessonResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var lesson = await _lessons.GetByIdAsync(request.LessonId, ct);
        if (lesson is null)
            return Result<LessonResponse>.Failure(Error.NotFound("lessons.not_found", $"Lesson {request.LessonId} not found."));

        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description?.Trim();
        lesson.Duration = request.Duration;
        lesson.Order = request.Order;
        lesson.IsPublished = request.IsPublished;
        lesson.UpdatedAt = _clock.UtcNow.UtcDateTime;

        _lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);

        return Result<LessonResponse>.Success(lesson.ToResponse());
    }
}

public sealed class PublishLessonHandler : IRequestHandler<PublishLessonCommand, Result<bool>>
{
    private readonly IRepository<Lesson> _lessons;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public PublishLessonHandler(
        IRepository<Lesson> lessons,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _lessons = lessons;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<bool>> Handle(PublishLessonCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var lesson = await _lessons.GetByIdAsync(request.LessonId, ct);
        if (lesson is null)
            return Result<bool>.Failure(Error.NotFound("lessons.not_found", $"Lesson {request.LessonId} not found."));

        lesson.IsPublished = request.IsPublished;
        lesson.UpdatedAt = _clock.UtcNow.UtcDateTime;

        _lessons.Update(lesson);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public sealed class DeleteLessonHandler : IRequestHandler<DeleteLessonCommand, Result<bool>>
{
    private readonly IRepository<Lesson> _lessons;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public DeleteLessonHandler(
        IRepository<Lesson> lessons,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _lessons = lessons;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteLessonCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var lesson = await _lessons.GetByIdAsync(request.LessonId, ct);
        if (lesson is null)
            return Result<bool>.Failure(Error.NotFound("lessons.not_found", $"Lesson {request.LessonId} not found."));

        _lessons.Remove(lesson);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

// ── Lesson Resources ──────────────────────────────────────────────────────

public sealed class AttachLessonResourceHandler : IRequestHandler<AttachLessonResourceCommand, Result<LessonResourceResponse>>
{
    private readonly IRepository<Lesson> _lessons;
    private readonly IRepository<LessonResource> _resources;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AttachLessonResourceHandler(
        IRepository<Lesson> lessons,
        IRepository<LessonResource> resources,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _lessons = lessons;
        _resources = resources;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<LessonResourceResponse>> Handle(AttachLessonResourceCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<LessonResourceResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var lesson = await _lessons.GetByIdAsync(request.LessonId, ct);
        if (lesson is null)
            return Result<LessonResourceResponse>.Failure(Error.NotFound("lessons.not_found", $"Lesson {request.LessonId} not found."));

        var resource = new LessonResource
        {
            LessonId = request.LessonId,
            ResourceType = request.ResourceType.Trim(),
            FileUrl = request.FileUrl.Trim(),
            FileName = request.FileName.Trim(),
            FileSizeBytes = request.FileSizeBytes,
            CreatedAt = _clock.UtcNow.UtcDateTime
        };

        await _resources.AddAsync(resource, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<LessonResourceResponse>.Success(resource.ToResponse());
    }
}

public sealed class RemoveLessonResourceHandler : IRequestHandler<RemoveLessonResourceCommand, Result<bool>>
{
    private readonly IRepository<LessonResource> _resources;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RemoveLessonResourceHandler(
        IRepository<LessonResource> resources,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _resources = resources;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(RemoveLessonResourceCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var resource = await _resources.GetByIdAsync(request.ResourceId, ct);
        if (resource is null)
            return Result<bool>.Failure(Error.NotFound("resources.not_found", $"Resource {request.ResourceId} not found."));

        _resources.Remove(resource);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
