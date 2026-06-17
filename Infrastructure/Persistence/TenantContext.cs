namespace OtpSystem.Infrastructure.Persistence;

public class TenantContext
{
    public Guid TenantId { get; private set; }
    public string TenantName { get; private set; } = default!;
    public string? WebhookUrl { get; private set; }
    private bool _isSet = false;

    public void Set(Guid id, string name, string? webhookUrl)
    {
        if (_isSet) throw new InvalidOperationException("Tenant already set for this request");
        TenantId = id;
        TenantName = name;
        WebhookUrl = webhookUrl;
        _isSet = true;
    }

    public bool IsSet => _isSet;
}