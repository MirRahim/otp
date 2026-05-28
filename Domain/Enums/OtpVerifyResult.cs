namespace OtpSystem.Domain.Enums;

public enum OtpVerifyResult
{
    Success,
    InvalidCode,
    Expired,
    AlreadyUsed,
    TooManyAttempts
}