namespace Pat.Services.Sms.Dto
{
    public class VerifySendModel
    {
        public string Mobile { get; set; }

        public int TemplateId { get; set; }

        public VerifySendParameterModel[] Parameters { get; set; }
    }
}
