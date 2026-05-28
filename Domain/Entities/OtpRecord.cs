using OtpSystem.Domain.Enums;
using System.Net.NetworkInformation;

namespace OtpSystem.Domain.Entities;

public class OtpRecord
{
    public Guid Id { get; private set; }
    public string Phone { get; private set; }
    public string Code { get; private set; }
    public OtpStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public object OttpVerifyResult { get; private set; }

    private const int MaxAttempts = 3;

    private OtpRecord() { } // EF Core

    public static OtpRecord Create(string phone, string code, TimeSpan ttl)
    {
        return new OtpRecord
        {
            Id = Guid.NewGuid(),
            Phone = phone,
            Code = code,
            Status = OtpStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl),
            AttemptCount = 0
        };
    }

    public OtpVerifyResult Verify(string inputCode)
    {
        if (Status != OtpStatus.Pending)
            return OtpVerifyResult.AlreadyUsed;

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = OtpStatus.Expired;
            return OtpVerifyResult.Expired;
        }

        AttemptCount++;

        if (AttemptCount > MaxAttempts)
        {
            Status = OtpStatus.Blocked;
            return OtpVerifyResult.TooManyAttempts;
        }

        if (Code != inputCode)
            return OtpVerifyResult.InvalidCode;

        Status = OtpStatus.Verified;
        return OtpVerifyResult.Success;
    }
}