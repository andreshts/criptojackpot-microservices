using FluentResults;

namespace CryptoJackpot.Domain.Core.Responses.Successes;

public class NoContentSuccess : Success
{
    public NoContentSuccess() : base(string.Empty) { }
}

