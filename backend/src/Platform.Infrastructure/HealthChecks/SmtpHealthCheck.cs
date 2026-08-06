using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Platform.Infrastructure.Notifications.Email;

namespace Platform.Infrastructure.HealthChecks;

public sealed class SmtpHealthCheck(IOptions<SmtpOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var smtp = options.Value;

        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(smtp.Host, smtp.Port, cancellationToken);

            return HealthCheckResult.Healthy($"SMTP is reachable at {smtp.Host}:{smtp.Port}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                $"SMTP is not reachable at {smtp.Host}:{smtp.Port}. Email sending may be unavailable.", ex);
        }
    }
}
