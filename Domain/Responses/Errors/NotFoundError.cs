namespace CryptoJackpot.Domain.Core.Responses.Errors;

public class NotFoundError(string message) : ApplicationError(message, 404);


