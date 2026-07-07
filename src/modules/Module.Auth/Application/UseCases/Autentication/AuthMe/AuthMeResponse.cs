using Common.Contracts.authentication.dtos;

namespace Module.Auth.Application.UseCases.Autentication.AuthMe;

public record AuthMeResponse(SessionStateDto SessionState);
