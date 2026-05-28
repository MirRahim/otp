namespace OtpSystem.Domain.Exceptions;

public class OtpDomainException : Exception
{
    public string Code { get; }

    public OtpDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}