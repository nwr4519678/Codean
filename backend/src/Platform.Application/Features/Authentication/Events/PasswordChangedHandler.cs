using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Messaging;
using Platform.Domain.Events.Authentication;

namespace Platform.Application.Features.Authentication.Events;

public sealed class PasswordChangedHandler(
    IEmailSender emailSender,
    ILogger<PasswordChangedHandler> logger)
    : IDomainEventHandler<PasswordChangedEvent>
{
    public async Task Handle(DomainEventWrapper<PasswordChangedEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        logger.LogInformation(
            "PasswordChanged: UserId={UserId}", e.UserId);

        await emailSender.SendAsync(new EmailMessage(
            To:       e.Email,
            Subject:  "Your password was changed",
            HtmlBody: $"""
                       <p>Your password was changed on <strong>{e.OccurredOn:f} UTC</strong>.</p>
                       <p>If you did not make this change, please contact support immediately.</p>
                       """,
            TextBody: $"Your password was changed on {e.OccurredOn:f} UTC. If this wasn't you, contact support."), ct);
    }
}
