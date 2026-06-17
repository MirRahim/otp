namespace OtpSystem.Application.Messaging;

public record SendOtpMessage
{
    public string? WebhookUrl;
    public Guid TenantId;
    public required string Phone { get; init; }
    public required string Code { get; init; }
    public required DateTime CreatedAt { get; init; }
}