namespace otpService.Services.OtpService
{
    public interface IOtpService
    {
        Task SendOtpAsync(string phone);
        Task<bool> VerifyOtpAsync(string phone, string code);
    }
}
