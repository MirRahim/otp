namespace OtpSystem.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string ApiKey { get; private set; } = default!;  // stored hashed
    public string? WebhookUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tenant() { } // EF Core

    public static Tenant Create(string name, string hashedApiKey, string? webhookUrl = null)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            ApiKey = hashedApiKey,
            WebhookUrl = webhookUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void UpdateWebhook(string url) => WebhookUrl = url;
}