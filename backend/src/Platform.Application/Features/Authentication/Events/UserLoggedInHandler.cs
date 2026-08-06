using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Messaging;
using Platform.Domain.Events.Authentication;

namespace Platform.Application.Features.Authentication.Events;

public sealed class UserLoggedInHandler(
    ILogger<UserLoggedInHandler> logger)
    : IDomainEventHandler<UserLoggedInEvent>
{
    public Task Handle(DomainEventWrapper<UserLoggedInEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        logger.LogInformation(
            "UserLoggedIn: UserId={UserId}, IP={IpAddress}, Device={DeviceInfo}, At={OccurredOn}",
            e.UserId, e.IpAddress, e.DeviceInfo, e.OccurredOn);

        // Extension point: detect new-IP login → send "New login from {IP}" security alert
        // Extension point: push notification to mobile app

        return Task.CompletedTask;
    }
}
