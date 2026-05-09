using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace otpService.Services.OtpService
{
    public class OtpService : IOtpService
    {
        private readonly IDatabase _redis;

        public OtpService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task SendOtpAsync(string phone)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();

            var key = $"otp:{phone}";
            var rateKey = $"rate:{phone}";

            // rate limit (مثلاً 3 تا در دقیقه)
            var count = await _redis.StringIncrementAsync(rateKey);
            if (count == 1)
                await _redis.KeyExpireAsync(rateKey, TimeSpan.FromMinutes(1));

            if (count > 3)
                throw new Exception("Too many requests");

            await _redis.StringSetAsync(
                key,
                otp,
                TimeSpan.FromMinutes(2)
            );

            // اینجا بعداً SMS service میاد
            Console.WriteLine($"OTP for {phone}: {otp}");
        }

        public async Task<bool> VerifyOtpAsync(string phone, string code)
        {
            var key = $"otp:{phone}";
            var storedOtp = await _redis.StringGetAsync(key);

            if (storedOtp.IsNullOrEmpty)
                return false;

            if (storedOtp != code)
                return false;

            await _redis.KeyDeleteAsync(key);

            return true;
        }

    }
}
