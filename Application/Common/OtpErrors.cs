namespace OtpSystem.Application.Common;

public static class OtpErrors
{
    public const string TooManyRequests = "OTP_001";
    public const string InvalidCode = "OTP_002";
    public const string Expired = "OTP_003";
    public const string AlreadyUsed = "OTP_004";
    public const string TooManyAttempts = "OTP_005";
    public const string SmsSendFailed = "OTP_006";
    public const string NotFound = "OTP_007";
}