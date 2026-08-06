using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.Notifications;

namespace Platform.Infrastructure.Notifications.Email;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "no-reply@platform.app";
    public string FromName { get; set; } = "Platform";
}

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> logger)
    {
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage m, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_opt.Host, _opt.Port, _opt.UseSsl, ct);
            if (!string.IsNullOrEmpty(_opt.Username))
                await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            msg.To.Add(MailboxAddress.Parse(m.To));
            msg.Subject = m.Subject;
            if (!string.IsNullOrEmpty(m.ReplyTo)) msg.ReplyTo.Add(MailboxAddress.Parse(m.ReplyTo));

            var body = new BodyBuilder
            {
                HtmlBody = m.HtmlBody,
                TextBody = m.TextBody ?? StripHtml(m.HtmlBody)
            };
            if (m.Attachments is not null)
            {
                foreach (var att in m.Attachments)
                    body.Attachments.Add(att.FileName, att.Content, ContentType.Parse(att.ContentType));
            }
            msg.Body = body.ToMessageBody();

            await client.SendAsync(msg, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed to {To}", m.To);
        }
    }

    public async Task SendManyAsync(IEnumerable<EmailMessage> messages, CancellationToken ct = default)
    {
        foreach (var m in messages) await SendAsync(m, ct);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }
}

public sealed class WebPushSender : IPushSender
{
    public Task SendAsync(PushMessage m, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class NoopSmsSender : ISmsSender
{
    public Task SendAsync(string to, string body, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class NoopWhatsAppSender : IWhatsAppSender
{
    public Task SendAsync(string to, string body, CancellationToken ct = default) => Task.CompletedTask;
}
