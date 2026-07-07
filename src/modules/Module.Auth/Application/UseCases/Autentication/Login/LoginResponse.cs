using Common.Contracts.authentication.dtos;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, SessionStateDto Session);
