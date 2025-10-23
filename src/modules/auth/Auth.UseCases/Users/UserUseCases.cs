using System;

namespace Auth.UseCases.Users;

public record UserUseCases(RegisterUser RegisterUser,
                             Login Login);