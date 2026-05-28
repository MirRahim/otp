using System.ComponentModel.DataAnnotations;

namespace OtpSystem.API.Controllers.Dto
{
    public class VerifyRequest
    {
        [Phone]
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}