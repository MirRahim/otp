namespace OtpSystem.Application.Messaging;

public record SendOtpMessage
{
    public required string Phone { get; init; }
    public required string Code { get; init; }
    public required DateTime CreatedAt { get; init; }
}