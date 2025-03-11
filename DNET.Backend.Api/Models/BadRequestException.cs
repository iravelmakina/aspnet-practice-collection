namespace DNET.Backend.Api.Models;

public class BadRequestException : Exception
{
    public BadRequestException(string wrongMessage, int wrongCode)
    {
        WrongMessage = wrongMessage;
        WrongCode = wrongCode;
    }

    public string WrongMessage { get; set; }
    public int WrongCode { get; set; }
}
