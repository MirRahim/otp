using System.ComponentModel.DataAnnotations;

namespace otpService.Controllers.Dto
{
    public class SendOtpDto
    {
        [Phone]
        public string Phone { get; set; }
    }
}