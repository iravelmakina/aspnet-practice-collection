namespace DNET.Backend.Api.Models;

public class ServerException : System.Exception
{
    public ServerException(string wrongMessage, int wrongCode)
    {
        WrongMessage = wrongMessage;
        WrongCode = wrongCode;
    }

    public string WrongMessage { get; set; }
    public int WrongCode { get; set; }
}
