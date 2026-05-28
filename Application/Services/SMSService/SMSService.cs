using OtpSystem.Application.Common;
using OtpSystem.Application.Services.SMSService.Dto;

using System.Text;
using System.Text.Json;

namespace OtpSystem.Application.Services.SMSService
{
    public class SMSService
    {
        private readonly HttpClient _http;
        public SMSService()
        {
            _http = new();
            _http.DefaultRequestHeaders.Add("x-api-key", "AppConsts.SmsApiToken");
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

        public Task SendAsync(string phone, string message)
        {
            Console.WriteLine($"*** Send SMS *** \n Sending SMS to {phone} with message: {message}");
            return Task.CompletedTask;
        }
    }
}
