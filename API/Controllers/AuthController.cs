using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OtpSystem.API.Controllers.Dto;
using OtpSystem.Application.Services.OtpService;
using System.Numerics;

namespace OtpSystem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly IOtpService _OtpSystem;
        public AuthController(IOtpService otp)
        {
            _OtpSystem = otp;
        }

        [HttpPost]
        [EnableRateLimiting("per-phone")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            await _OtpSystem.SendOtpAsync(dto.Phone);
            return Ok("OTP sent");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyRequest req)
        {
            var result = await _OtpSystem.VerifyOtpAsync(req.Phone, req.Code);
            return Ok(result);
        }
    }
}
