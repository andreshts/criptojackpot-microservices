using CryptoJackpot.Domain.Core.Responses.Successes;
using FluentResults;

namespace CryptoJackpot.Domain.Core.Responses;

/// <summary>
/// Helper methods to create Result with typed successes.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Creates a successful Result with 201 Created status.
    /// </summary>
    public static Result<T> Created<T>(T value)
    {
        return Result.Ok(value).WithSuccess(new CreatedSuccess());
    }
    
    /// <summary>
    /// Creates a successful Result with 204 No Content status.
    /// </summary>
    public static Result<T> NoContent<T>()
    {
        return Result.Ok(default(T)!).WithSuccess(new NoContentSuccess());
    }
}

