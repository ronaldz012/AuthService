namespace Module.Auth.Application.UseCases.Users.CreateUser;

public record CreateUserResponse(Guid UserId, string SetupUrl, bool EmailSent);
