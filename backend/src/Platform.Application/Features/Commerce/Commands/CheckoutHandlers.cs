using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Payments;
using Platform.Application.Features.Commerce.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Commerce.Commands;

public sealed class InitiateCheckoutHandler
    : IRequestHandler<InitiateCheckoutCommand, Result<CheckoutResponse>>
{
    private readonly IRepository<SubscriptionPlan> _plans;
    private readonly IRepository<Payment> _payments;
    private readonly IUnitOfWork _uow;
    private readonly IPaymobClient _paymob;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public InitiateCheckoutHandler(
        IRepository<SubscriptionPlan> plans,
        IRepository<Payment> payments,
        IUnitOfWork uow,
        IPaymobClient paymob,
        ICurrentUser currentUser,
        IClock clock)
    {
        _plans       = plans;
        _payments    = payments;
        _uow         = uow;
        _paymob      = paymob;
        _currentUser = currentUser;
        _clock       = clock;
    }

    public async Task<Result<CheckoutResponse>> Handle(
        InitiateCheckoutCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CheckoutResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var plan = await _plans.GetByIdAsync(request.PlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result<CheckoutResponse>.Failure(
                Error.NotFound("plans.not_found", "Active subscription plan not found."));

        var merchantOrderId = $"sub_plan_{plan.Id}_{studentId}_{Guid.NewGuid():N}";
        int amountCents = (int)(plan.Price * 100);

        var items = new List<(string name, int qty, decimal amountCents)>
        {
            (plan.Name, 1, amountCents)
        };

        // Call Paymob API to create intention and hosted checkout URL
        var paymobResult = await _paymob.CreateIntentionAsync(
            merchantOrderId,
            plan.Price,
            "EGP",
            items,
            studentId,
            couponCode: null,
            ct);

        if (!paymobResult.IsSuccess)
            return Result<CheckoutResponse>.Failure(paymobResult.Error);

        var intention = paymobResult.Value!;

        // Record pending payment in DB
        var payment = new Payment
        {
            StudentId     = studentId,
            TransactionId = merchantOrderId,
            Amount        = plan.Price,
            Currency      = "EGP",
            Status        = "Pending",
            PaymentMethod = "Paymob",
            CreatedAt     = _clock.UtcNow.UtcDateTime
        };

        await _payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CheckoutResponse>.Success(new CheckoutResponse(
            merchantOrderId,
            intention.CheckoutUrl,
            intention.ClientSecret,
            plan.Price,
            "EGP"
        ));
    }
}

public sealed class InitiateCourseCheckoutHandler
    : IRequestHandler<InitiateCourseCheckoutCommand, Result<CheckoutResponse>>
{
    private readonly IRepository<Course> _courses;
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<CourseEnrollment> _enrollments;
    private readonly IUnitOfWork _uow;
    private readonly IPaymobClient _paymob;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public InitiateCourseCheckoutHandler(
        IRepository<Course> courses,
        IRepository<Payment> payments,
        IRepository<CourseEnrollment> enrollments,
        IUnitOfWork uow,
        IPaymobClient paymob,
        ICurrentUser currentUser,
        IClock clock)
    {
        _courses = courses;
        _payments = payments;
        _enrollments = enrollments;
        _uow = uow;
        _paymob = paymob;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<CheckoutResponse>> Handle(
        InitiateCourseCheckoutCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CheckoutResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var studentId = _currentUser.UserId.Value;
        var course = await _courses.Query()
            .Include(c => c.CourseEnrollments)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.IsPublished, ct);

        if (course is null)
            return Result<CheckoutResponse>.Failure(
                Error.NotFound("courses.not_found", "Published course not found."));

        if (course.Price <= 0)
            return Result<CheckoutResponse>.Failure(
                Error.Validation("courses.free_course", "Free courses should use free enrollment."));

        var enrollment = course.CourseEnrollments
            .FirstOrDefault(e => e.StudentId == studentId && e.Status == "Active");
        if (enrollment is not null)
            return Result<CheckoutResponse>.Failure(
                Error.Conflict("courses.already_enrolled", "You already have access to this course."));

        var merchantOrderId = $"course_{course.Id}_{studentId}_{Guid.NewGuid():N}";
        var amountCents = (int)Math.Round(course.Price * 100, MidpointRounding.AwayFromZero);
        var items = new List<(string name, int qty, decimal amountCents)>
        {
            (course.Title, 1, amountCents)
        };

        var paymobResult = await _paymob.CreateIntentionAsync(
            merchantOrderId,
            course.Price,
            "EGP",
            items,
            studentId,
            couponCode: null,
            ct);

        if (!paymobResult.IsSuccess)
            return Result<CheckoutResponse>.Failure(paymobResult.Error);

        var intention = paymobResult.Value!;
        var payment = new Payment
        {
            StudentId = studentId,
            TransactionId = merchantOrderId,
            Amount = course.Price,
            Currency = "EGP",
            Status = "Pending",
            PaymentMethod = "Paymob",
            CreatedAt = _clock.UtcNow.UtcDateTime
        };
        var pendingEnrollment = new CourseEnrollment
        {
            CourseId = course.Id,
            StudentId = studentId,
            Status = "Pending",
            AccessType = "Paid",
            EnrolledAt = _clock.UtcNow.UtcDateTime,
            Payment = payment
        };

        await _payments.AddAsync(payment, ct);
        await _enrollments.AddAsync(pendingEnrollment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CheckoutResponse>.Success(new CheckoutResponse(
            merchantOrderId,
            intention.CheckoutUrl,
            intention.ClientSecret,
            course.Price,
            "EGP"));
    }
}
