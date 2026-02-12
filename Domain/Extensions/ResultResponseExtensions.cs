using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Domain.Core.Responses.Successes;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Domain.Core.Extensions;

/// <summary>
/// Shared extension methods for converting FluentResults Result to IActionResult.
/// Used across all microservices for consistent API responses.
/// </summary>
public static class ResultResponseExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            var error = result.Errors.FirstOrDefault();
            
            var statusCode = error switch
            {
                ApplicationError appError => appError.StatusCode,
                _ => StatusCodes.Status500InternalServerError
            };

            // Handle validation errors with detailed field errors
            if (error is ValidationError validationError)
            {
                return new ObjectResult(new
                {
                    success = false,
                    message = error.Message,
                    errors = validationError.Errors
                })
                {
                    StatusCode = statusCode
                };
            }

            // Handle locked errors with Retry-After header
            if (error is LockedError lockedError)
            {
                var objectResult = new ObjectResult(new
                {
                    success = false,
                    message = error.Message,
                    retryAfterSeconds = lockedError.RetryAfterSeconds
                })
                {
                    StatusCode = statusCode
                };
                return objectResult;
            }

            return new ObjectResult(new { success = false, message = error?.Message ?? "An error occurred" })
            {
                StatusCode = statusCode
            };
        }

        // Check for typed successes
        var success = result.Successes.FirstOrDefault();
        
        return success switch
        {
            NoContentSuccess => new NoContentResult(),
            CreatedSuccess => new ObjectResult(new { success = true, data = result.Value })
            {
                StatusCode = StatusCodes.Status201Created
            },
            _ => new OkObjectResult(new { success = true, data = result.Value })
        };
    }

    /// <summary>
    /// Converts a non-generic Result to IActionResult.
    /// Used for commands that don't return a value.
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsFailed)
        {
            var error = result.Errors.FirstOrDefault();
            
            var statusCode = error switch
            {
                ApplicationError appError => appError.StatusCode,
                _ => StatusCodes.Status500InternalServerError
            };

            // Handle validation errors with detailed field errors
            if (error is ValidationError validationError)
            {
                return new ObjectResult(new
                {
                    success = false,
                    message = error.Message,
                    errors = validationError.Errors
                })
                {
                    StatusCode = statusCode
                };
            }

            // Handle locked errors with Retry-After header
            if (error is LockedError lockedError)
            {
                return new ObjectResult(new
                {
                    success = false,
                    message = error.Message,
                    retryAfterSeconds = lockedError.RetryAfterSeconds
                })
                {
                    StatusCode = statusCode
                };
            }

            return new ObjectResult(new { success = false, message = error?.Message ?? "An error occurred" })
            {
                StatusCode = statusCode
            };
        }

        // Check for typed successes
        var success = result.Successes.FirstOrDefault();
        
        return success switch
        {
            NoContentSuccess => new NoContentResult(),
            _ => new OkObjectResult(new { success = true })
        };
    }
}

