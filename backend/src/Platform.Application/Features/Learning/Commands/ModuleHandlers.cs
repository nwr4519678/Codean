using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Caching;
using Platform.Application.Features.Learning.Dtos;
using Platform.Application.Features.Learning.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Learning.Commands;

public sealed class CreateCourseModuleHandler : IRequestHandler<CreateCourseModuleCommand, Result<CourseModuleResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IRepository<CourseModule> _modules;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public CreateCourseModuleHandler(
        IRepository<Course> courses,
        IRepository<CourseModule> modules,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock,
        ICacheService cache)
    {
        _courses = courses;
        _modules = modules;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
        _cache = cache;
    }

    public async Task<Result<CourseModuleResponse>> Handle(CreateCourseModuleCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseModuleResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<CourseModuleResponse>.Failure(Error.NotFound("courses.not_found", $"Course {request.CourseId} not found."));

        if (course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<CourseModuleResponse>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission."));

        var module = new CourseModule
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            MonthNumber = request.MonthNumber,
            Order = request.Order,
            Description = request.Description?.Trim(),
            CreatedAt = _clock.UtcNow.UtcDateTime
        };

        await _modules.AddAsync(module, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.CourseDetail(request.CourseId), ct);

        return Result<CourseModuleResponse>.Success(module.ToResponse());
    }
}

public sealed class UpdateCourseModuleHandler : IRequestHandler<UpdateCourseModuleCommand, Result<CourseModuleResponse>>
{
    private readonly IRepository<CourseModule> _modules;
    private readonly IRepository<Course> _courses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public UpdateCourseModuleHandler(
        IRepository<CourseModule> modules,
        IRepository<Course> courses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _modules = modules;
        _courses = courses;
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<CourseModuleResponse>> Handle(UpdateCourseModuleCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CourseModuleResponse>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var module = await _modules.GetByIdAsync(request.ModuleId, ct);
        if (module is null)
            return Result<CourseModuleResponse>.Failure(Error.NotFound("modules.not_found", $"Module {request.ModuleId} not found."));

        var course = await _courses.GetByIdAsync(module.CourseId, ct);
        if (course is not null && course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<CourseModuleResponse>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission."));

        module.Title = request.Title.Trim();
        module.MonthNumber = request.MonthNumber;
        module.Order = request.Order;
        module.Description = request.Description?.Trim();

        _modules.Update(module);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.CourseDetail(module.CourseId), ct);

        return Result<CourseModuleResponse>.Success(module.ToResponse());
    }
}

public sealed class DeleteCourseModuleHandler : IRequestHandler<DeleteCourseModuleCommand, Result<bool>>
{
    private readonly IRepository<CourseModule> _modules;
    private readonly IRepository<Course> _courses;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cache;

    public DeleteCourseModuleHandler(
        IRepository<CourseModule> modules,
        IRepository<Course> courses,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ICacheService cache)
    {
        _modules = modules;
        _courses = courses;
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(DeleteCourseModuleCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var module = await _modules.GetByIdAsync(request.ModuleId, ct);
        if (module is null)
            return Result<bool>.Failure(Error.NotFound("modules.not_found", $"Module {request.ModuleId} not found."));

        var course = await _courses.GetByIdAsync(module.CourseId, ct);
        if (course is not null && course.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<bool>.Failure(Error.Forbidden("auth.forbidden", "You do not have permission."));

        _modules.Remove(module);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.CourseDetail(module.CourseId), ct);

        return Result<bool>.Success(true);
    }
}
