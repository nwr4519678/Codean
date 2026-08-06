using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Messaging;
using Platform.Domain.Events.Authentication;

namespace Platform.Application.Features.Authentication.Events;

public sealed class UserRegisteredHandler(
    IEmailSender emailSender,
    ILogger<UserRegisteredHandler> logger)
    : IDomainEventHandler<UserRegisteredEvent>
{
    public async Task Handle(DomainEventWrapper<UserRegisteredEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        logger.LogInformation(
            "UserRegistered: UserId={UserId}, Email={Email}", e.UserId, e.Email);

        await emailSender.SendAsync(new EmailMessage(
            To:       e.Email,
            Subject:  "Welcome to the Platform!",
            HtmlBody: $"""
                       <h1>Welcome, {e.FullName}!</h1>
                       <p>Your account has been created. Please verify your email to get started.</p>
                       """,
            TextBody: $"Welcome, {e.FullName}! Please verify your email to get started."), ct);
    }
}
