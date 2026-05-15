using System.ComponentModel.DataAnnotations;

namespace otpService.Controllers.Dto
{
    public class VerifyRequest
    {
        [Phone]
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}