namespace CryptoJackpot.Domain.Core.Responses.Errors;

public class InternalServerError(string message) : ApplicationError(message, 500);

