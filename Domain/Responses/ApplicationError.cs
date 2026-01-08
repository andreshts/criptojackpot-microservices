using FluentResults;

namespace CryptoJackpot.Domain.Core.Responses;

public class ApplicationError : Error
{
    public int StatusCode { get; init; }

    protected ApplicationError(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
        Metadata.Add("ErrorCode", statusCode);
    }
}
