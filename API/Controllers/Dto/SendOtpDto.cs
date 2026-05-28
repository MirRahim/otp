using System.ComponentModel.DataAnnotations;

namespace OtpSystem.API.Controllers.Dto
{
    public class SendOtpDto
    {
        [Phone]
        public string Phone { get; set; }
    }
}