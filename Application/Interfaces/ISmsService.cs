namespace OtpSystem.Application.Interfaces;

public interface ISmsService
{
    Task<bool> SendOtpAsync(string phone, string code);
}