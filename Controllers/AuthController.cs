using Microsoft.AspNetCore.Mvc;
using otpService.Services.OtpService;
using System.Numerics;

namespace otpService.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly IOtpService _otpService;
        public AuthController(IOtpService otp)
        {
            _otpService = otp;
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] string phone)
        {
            await _otpService.SendOtpAsync(phone);
            return Ok("OTP sent");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRequest req)
        {
            var result = await _otpService.VerifyOtpAsync(req.Phone, req.Code);
            return Ok(result);
        }
    }
}
