namespace OtpSystem.Application.Services.OtpService
{
    public interface IOtpService
    {
        Task<bool> SendOtpAsync(string phone);
        Task<bool> VerifyOtpAsync(string phone, string code);
    }
}
