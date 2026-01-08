using FluentResults;

namespace CryptoJackpot.Domain.Core.Responses.Successes;

public class CreatedSuccess : Success
{
    public CreatedSuccess(string message = "Resource created successfully") : base(message) { }
}
