using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Messaging;
using Platform.Domain.Events.Authentication;

namespace Platform.Application.Features.Authentication.Events;

public sealed class EmailVerifiedHandler(
    IEmailSender emailSender,
    ILogger<EmailVerifiedHandler> logger)
    : IDomainEventHandler<EmailVerifiedEvent>
{
    public async Task Handle(DomainEventWrapper<EmailVerifiedEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        logger.LogInformation(
            "EmailVerified: UserId={UserId}, Email={Email}", e.UserId, e.Email);

        await emailSender.SendAsync(new EmailMessage(
            To:       e.Email,
            Subject:  "Email Verified ✓",
            HtmlBody: "<p>Your email address has been verified. Your account is now fully active.</p>",
            TextBody: "Your email address has been verified. Your account is now fully active."), ct);
    }
}
