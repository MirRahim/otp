using Microsoft.AspNetCore.Mvc;

namespace otpService.Controllers
{
    public class OtpController : Controller
    {
        public async Task<IActionResult> SendOtp()
        {
            return Ok();
        }

        public async Task<IActionResult> VerifyOtp()
        {
            return Ok();
        }
    }
}
