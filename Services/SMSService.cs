using Microsoft.Extensions.Caching.Memory;
using otpService.Common;
using Pat.Services.Sms.Dto;
using System.Text;
using System.Text.Json;

namespace otpService.Services.Sms
{
    public class SMSService
    {        
        private readonly IMemoryCache _cache;
        private readonly HttpClient _http = new();
        public SMSService(IMemoryCache cache)
        {
            _cache = cache;
            _http.DefaultRequestHeaders.Add("x-api-key", AppConsts.SmsApiToken);
        }
        public async Task<bool> SendOTPSms(string phone, string code)
        {
            VerifySendModel model = new()
            {
                Mobile = phone,
                TemplateId = AppConsts.Sms_ir_OTP_TemplateId,
                Parameters = [
                    new VerifySendParameterModel {
                        Name = "CODE",
                        Value = code
                    }
                ]
            };

            string payload = JsonSerializer.Serialize(model);
            StringContent stringContent = new(payload, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("https://api.sms.ir/v1/send/verify", stringContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"sms.ir error: {phone}, {code}, resp: {JsonSerializer.Serialize(response)}");
                return false;
            }
            return true;
        }
        public async Task<string> GenerateOtp(string phone)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _cache.Set($"otp:{phone}", code, TimeSpan.FromMinutes(2));
            //var result = await SendCustomTemplateSms(phone, "mynetro", code);
            var sent = await SendOTPSms(phone, code);
            if (!sent) return null;

            Console.WriteLine($"OTP for {phone}: {code}"); // فقط برای تست
            return code;
        }

        public bool VerifyOtp(string phone, string code)
        {
            if (_cache.TryGetValue<string>($"otp:{phone}", out var expectedCode))
            {
                if (expectedCode == code)
                {
                    _cache.Remove($"otp:{phone}");
                    return true;
                }
            }
            return false;
        }

        public Task SendAsync(string phone, string message)
        {
            Console.WriteLine($"*** Send SMS *** \n Sending SMS to {phone} with message: {message}");
            return Task.CompletedTask;
        }
    }
}
