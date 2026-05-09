using Microsoft.AspNetCore.Mvc;

namespace otpService.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OtpController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> SendOtp()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp()
        {
            return Ok();
        }
    }
}
