using OtpSystem.Application.Common;
using static MassTransit.ValidationResultExtensions;
using Result = OtpSystem.Application.Common.Result;

namespace OtpSystem.Application.Services.OtpService
{
    public interface IOtpService
    {
        Task<Result> SendOtpAsync(string phone);
        Task<Result<bool>> VerifyOtpAsync(string phone, string code);
    }
}
