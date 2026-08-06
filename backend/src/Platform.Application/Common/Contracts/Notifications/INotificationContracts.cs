namespace Platform.Application.Common.Contracts.Notifications;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? ReplyTo = null,
    IReadOnlyList<EmailAttachment>? Attachments = null);

public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType);

public sealed record PushMessage(
    string Endpoint,
    string? P256dh,
    string? Auth,
    string Title,
    string Body,
    string? Url = null);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
    Task SendManyAsync(IEnumerable<EmailMessage> messages, CancellationToken ct = default);
}

public interface IPushSender
{
    Task SendAsync(PushMessage message, CancellationToken ct = default);
}

public interface ISmsSender
{
    Task SendAsync(string to, string body, CancellationToken ct = default);
}

public interface IWhatsAppSender
{
    Task SendAsync(string to, string body, CancellationToken ct = default);
}
