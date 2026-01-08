namespace CryptoJackpot.Domain.Core.Responses.Errors;
public class ForbiddenError(string message) : ApplicationError(message, 403);


