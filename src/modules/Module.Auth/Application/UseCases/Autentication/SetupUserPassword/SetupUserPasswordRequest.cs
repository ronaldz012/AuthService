namespace Module.Auth.Application.UseCases.Autentication.SetupUserPassword;

public record SetupUserPasswordRequest(
    string Token,
    string Password
);