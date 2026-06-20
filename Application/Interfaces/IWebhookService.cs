namespace OtpSystem.Application.Interfaces;

public interface IWebhookService
{
    Task SendAsync(string webhookUrl, WebhookPayload payload);
}

public class WebhookPayload
{
    public string Event { get; init; } = default!;   // "otp.sent" | "otp.failed"
    public Guid TenantId { get; init; }
    public string Phone { get; init; } = default!;
    public bool Success { get; init; }
    public DateTime Timestamp { get; init; }
}